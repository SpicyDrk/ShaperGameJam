using System.Collections.Generic;
using ShapeConnections.Data;
using ShapeConnections.Game.Grid;
using ShapeConnections.Game.Wiring;
using ShapeConnections.Simulation;
using ShapeConnections.Simulation.GameLoop;
using ShapeConnections.Simulation.Grid;
using UnityEngine;
using UnityEngine.Events;

namespace ShapeConnections.Game.Loop
{
    /// <summary>
    /// Thin Unity adapter that owns the scene-side wiring of the core loop.
    /// All non-UI logic delegates to <see cref="LoopOrchestrator"/> + the
    /// IRunTrigger / IGraphBuilder / IWinCondition interfaces, so this class
    /// stays small and the loop stays swappable.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelRunner : MonoBehaviour
    {
        [SerializeField] private LevelDefinition level;
        [SerializeField] private GridView gridView;
        [SerializeField] private WireDragController wires;
        [SerializeField] private Transform nodesRoot;
        [SerializeField] private MonoBehaviour runTriggerBehaviour; // must implement IRunTrigger
        [SerializeField] private float cellSize = 1f;
        [SerializeField, Tooltip("Spacing in world units from the grid edge to the row of input/output sockets.")]
        private float socketEdgeMargin = 0.75f;

        [Header("Events")]
        public UnityEvent<LoopResult> LevelCompleted = new UnityEvent<LoopResult>();

        private readonly Dictionary<int, JackView> _inputSockets  = new Dictionary<int, JackView>();
        private readonly Dictionary<int, JackView> _outputSockets = new Dictionary<int, JackView>();
        private LoopOrchestrator _orchestrator;
        private IRunTrigger _trigger;

        public LevelDefinition Level => level;
        public IReadOnlyDictionary<int, JackView> InputSockets  => _inputSockets;
        public IReadOnlyDictionary<int, JackView> OutputSockets => _outputSockets;

#if UNITY_EDITOR
        /// <summary>Editor-only direct-assignment hook used by SceneScaffolder.</summary>
        public void EditorConfigure(LevelDefinition level, GridView gridView, WireDragController wires,
            Transform nodesRoot, MonoBehaviour runTriggerBehaviour)
        {
            this.level = level;
            this.gridView = gridView;
            this.wires = wires;
            this.nodesRoot = nodesRoot;
            this.runTriggerBehaviour = runTriggerBehaviour;
        }
#endif

        private void Awake()
        {
            // Edit-time AddComponent fires Awake before the scaffolder assigns serialized fields.
            // Defer all init (and the "enabled = false" trap) to Play.
            if (!Application.isPlaying) return;

            if (level == null) { Debug.LogError("[LevelRunner] No LevelDefinition assigned."); enabled = false; return; }
            if (gridView == null) { Debug.LogError("[LevelRunner] No GridView assigned."); enabled = false; return; }
            if (wires == null) { Debug.LogError("[LevelRunner] No WireDragController assigned."); enabled = false; return; }

            _trigger = runTriggerBehaviour as IRunTrigger;
            if (_trigger == null)
            {
                Debug.LogWarning("[LevelRunner] runTriggerBehaviour does not implement IRunTrigger — Run button will not fire.");
            }

            gridView.Bind(level.GridWidth, level.GridHeight, cellSize);
            SpawnEdgeSockets();

            var builder = new SceneGraphBuilder(level, nodesRoot != null ? nodesRoot : transform, wires, _inputSockets, _outputSockets);
            _orchestrator = new LoopOrchestrator(builder, new ExactMatchWinCondition());
        }

        private void OnEnable()
        {
            if (_trigger != null) _trigger.RunRequested += HandleRun;
        }

        private void OnDisable()
        {
            if (_trigger != null) _trigger.RunRequested -= HandleRun;
        }

        private void HandleRun()
        {
            var targets = new Shape[level.Targets.Count];
            for (var i = 0; i < level.Targets.Count; i++)
            {
                targets[i] = level.Targets[i] != null ? level.Targets[i].ToShape() : default;
            }

            var result = _orchestrator.Run(targets);
            LevelCompleted.Invoke(result);
        }

        private void SpawnEdgeSockets()
        {
            // Inputs on the left edge, outputs on the right edge — same metaphor as
            // a synth panel's left-input / right-output convention.
            var layout = gridView.Layout;
            var inputX  = layout.WorldPosition(new GridCoord(0, 0)).x - socketEdgeMargin;
            var outputX = layout.WorldPosition(new GridCoord(layout.Width - 1, 0)).x + socketEdgeMargin;

            for (var i = 0; i < level.Inputs.Count; i++)
            {
                var row = MapSocketToRow(i, level.Inputs.Count, layout.Height);
                var y = layout.WorldPosition(new GridCoord(0, row)).y;
                var jack = SpawnSocket($"InSocket-{i}", new Vector3(inputX, y, 0f), slot: i, isInput: false);
                // From the graph's POV the level Source has output jacks (its outputs feed into nodes).
                _inputSockets[i] = jack;
            }

            for (var i = 0; i < level.Targets.Count; i++)
            {
                var row = MapSocketToRow(i, level.Targets.Count, layout.Height);
                var y = layout.WorldPosition(new GridCoord(0, row)).y;
                var jack = SpawnSocket($"OutSocket-{i}", new Vector3(outputX, y, 0f), slot: i, isInput: true);
                // From the graph's POV the level Sink has input jacks.
                _outputSockets[i] = jack;
            }
        }

        private static int MapSocketToRow(int index, int count, int gridHeight)
        {
            if (count <= 1) return gridHeight / 2;
            // Spread across rows; clamp to grid bounds.
            var t = index / (float)(count - 1);
            var row = Mathf.RoundToInt(t * (gridHeight - 1));
            return Mathf.Clamp(row, 0, gridHeight - 1);
        }

        private const float SocketWorldRadius = 0.18f;
        private const int SocketSortingOrder = 10;

        private JackView SpawnSocket(string name, Vector3 worldPos, int slot, bool isInput)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, worldPositionStays: false);
            go.transform.position = worldPos;
            // Sprite is 1×1 local; localScale sets the visible diameter (≈ 2 × world radius).
            go.transform.localScale = Vector3.one * (SocketWorldRadius * 2f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = SocketSortingOrder;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f; // matches sprite radius in local units
            col.isTrigger = true;

            var jack = go.AddComponent<JackView>();
            // Synthetic node ids tag this jack as belonging to the level source/sink.
            // SceneGraphBuilder translates them to Graph.SourceNodeId / SinkNodeId.
            var syntheticId = isInput ? $"__sink__::{slot}" : $"__source__::{slot}";
            jack.Initialize(syntheticId, slotIndex: slot, isInput: isInput);
            return jack;
        }
    }
}
