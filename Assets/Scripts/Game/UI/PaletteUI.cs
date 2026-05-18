using System.Collections.Generic;
using ShapeConnections.Data;
using ShapeConnections.Game.Grid;
using ShapeConnections.Game.Wiring;
using ShapeConnections.Simulation.Grid;
using UnityEngine;
using UnityEngine.UI;

namespace ShapeConnections.Game.UI
{
    /// <summary>
    /// Minimal Phase-2 palette: one Button per palette entry. Clicking spawns
    /// a <see cref="NodeView"/> on the grid at the next available cell.
    /// Drag-and-drop placement is intentionally deferred — Phase 2 ships the
    /// place-then-wire-then-Run flow with click-to-place to keep scope tight.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PaletteUI : MonoBehaviour
    {
        [SerializeField] private LevelDefinition level;
        [SerializeField] private RectTransform buttonContainer;
        [SerializeField] private Button buttonTemplate;
        [SerializeField] private GridView gridView;
        [SerializeField] private Transform nodesRoot;

        private readonly Dictionary<NodeDefinition, int> _remaining = new Dictionary<NodeDefinition, int>();
        private readonly Dictionary<NodeDefinition, Button> _buttons = new Dictionary<NodeDefinition, Button>();
        private int _nodeCounter;
        private readonly HashSet<GridCoord> _occupied = new HashSet<GridCoord>();

#if UNITY_EDITOR
        /// <summary>Editor-only direct-assignment hook used by SceneScaffolder.</summary>
        public void EditorConfigure(LevelDefinition level, RectTransform buttonContainer,
            Button buttonTemplate, GridView gridView, Transform nodesRoot)
        {
            this.level = level;
            this.buttonContainer = buttonContainer;
            this.buttonTemplate = buttonTemplate;
            this.gridView = gridView;
            this.nodesRoot = nodesRoot;
        }
#endif

        private void Awake()
        {
            // The scaffolder calls AddComponent during edit mode; Awake fires before
            // serialized fields are assigned. Skip validation until Play to avoid a
            // spurious edit-time error and to keep the component enabled in the saved scene.
            if (!Application.isPlaying) return;

            if (level == null || buttonContainer == null || buttonTemplate == null || gridView == null)
            {
                Debug.LogError("[PaletteUI] Missing required references.");
                enabled = false;
                return;
            }

            buttonTemplate.gameObject.SetActive(false);

            foreach (var entry in level.Palette)
            {
                if (entry == null || entry.Node == null || entry.Count <= 0) continue;
                if (_remaining.ContainsKey(entry.Node)) _remaining[entry.Node] += entry.Count;
                else _remaining[entry.Node] = entry.Count;
            }

            foreach (var kvp in _remaining)
            {
                var node = kvp.Key;
                var btn = Instantiate(buttonTemplate, buttonContainer);
                btn.gameObject.SetActive(true);
                btn.name = $"PaletteButton-{node.DisplayName}";
                btn.onClick.AddListener(() => OnPaletteClicked(node));
                _buttons[node] = btn;
                UpdateButtonLabel(node);
            }
        }

        private void OnPaletteClicked(NodeDefinition node)
        {
            if (!_remaining.TryGetValue(node, out var count) || count <= 0) return;

            var cell = FindNextFreeCell();
            if (cell == null)
            {
                Debug.LogWarning("[PaletteUI] No free cells available on the grid.");
                return;
            }

            var worldPos = gridView.WorldPositionOf(cell.Value);
            var go = new GameObject($"{node.DisplayName}-{_nodeCounter++}");
            go.transform.SetParent(nodesRoot != null ? nodesRoot : gridView.transform, worldPositionStays: false);
            go.transform.position = worldPos;

            var view = go.AddComponent<NodeView>();
            view.Bind(node, $"node-{_nodeCounter}");
            _occupied.Add(cell.Value);

            _remaining[node] = count - 1;
            UpdateButtonLabel(node);
        }

        private GridCoord? FindNextFreeCell()
        {
            // Start at the grid centre and spiral outward — feels nicer than top-left fill.
            var layout = gridView.Layout;
            var cx = layout.Width  / 2;
            var cy = layout.Height / 2;
            var maxRadius = Mathf.Max(layout.Width, layout.Height);

            for (var r = 0; r <= maxRadius; r++)
            {
                for (var dy = -r; dy <= r; dy++)
                for (var dx = -r; dx <= r; dx++)
                {
                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != r) continue;
                    var coord = new GridCoord(cx + dx, cy + dy);
                    if (!layout.InBounds(coord)) continue;
                    if (_occupied.Contains(coord)) continue;
                    return coord;
                }
            }
            return null;
        }

        private void UpdateButtonLabel(NodeDefinition node)
        {
            if (!_buttons.TryGetValue(node, out var btn)) return;
            var label = btn.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = $"{node.DisplayName}  ×{_remaining[node]}";
            }
            btn.interactable = _remaining[node] > 0;
        }
    }
}
