using System.Collections.Generic;
using ShapeConnections.Data;
using UnityEngine;

namespace ShapeConnections.Game.Wiring
{
    /// <summary>
    /// Visual representation of a placed node. Spawns input/output
    /// <see cref="JackView"/> children based on the bound
    /// <see cref="NodeDefinition"/>. The view never holds the runtime
    /// <see cref="Simulation.Node"/> — graph assembly reads NodeViews via
    /// <see cref="IGraphBuilder"/> at Run time.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NodeView : MonoBehaviour
    {
        [SerializeField] private Sprite bodySprite;
        [SerializeField] private Color bodyColor = new Color(0.25f, 0.25f, 0.28f, 1f);
        [SerializeField] private float bodyHalfWidth = 0.35f;
        [SerializeField] private float jackOffset = 0.45f;
        [SerializeField] private float jackRadius = 0.14f; // visual + click radius — bumped from 0.08 for easier targeting

        private const int BodySortingOrder = 5;
        private const int JackSortingOrder = 10;

        private readonly List<JackView> _inputJacks  = new List<JackView>();
        private readonly List<JackView> _outputJacks = new List<JackView>();
        private NodeDefinition _definition;

        public string NodeId { get; private set; }
        public NodeDefinition Definition => _definition;
        public IReadOnlyList<JackView> InputJacks  => _inputJacks;
        public IReadOnlyList<JackView> OutputJacks => _outputJacks;

        public void Bind(NodeDefinition definition, string nodeId)
        {
            if (definition == null) throw new System.ArgumentNullException(nameof(definition));
            if (string.IsNullOrEmpty(nodeId)) throw new System.ArgumentException("nodeId required", nameof(nodeId));

            _definition = definition;
            NodeId = nodeId;
            name = $"Node[{definition.DisplayName}:{nodeId}]";

            ClearChildren();
            SpawnBody();
            SpawnJacks(_inputJacks, definition.InputSlotCount, isInput: true,  xOffset: -jackOffset);
            SpawnJacks(_outputJacks, definition.OutputSlotCount, isInput: false, xOffset:  jackOffset);
        }

        private void ClearChildren()
        {
            _inputJacks.Clear();
            _outputJacks.Clear();
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }

        private void SpawnBody()
        {
            var body = new GameObject("Body");
            body.transform.SetParent(transform, worldPositionStays: false);
            var sr = body.AddComponent<SpriteRenderer>();
            sr.sprite = bodySprite != null ? bodySprite : RoundedSquare();
            sr.color = bodyColor;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(bodyHalfWidth * 2f, bodyHalfWidth * 2f);
            sr.sortingOrder = BodySortingOrder;
        }

        private void SpawnJacks(List<JackView> bucket, int count, bool isInput, float xOffset)
        {
            if (count <= 0) return;
            // Distribute vertically, centered.
            var spacing = count > 1 ? (bodyHalfWidth * 1.6f) / (count - 1) : 0f;
            var startY = count > 1 ? -bodyHalfWidth * 0.8f : 0f;

            for (var i = 0; i < count; i++)
            {
                var go = new GameObject();
                go.transform.SetParent(transform, worldPositionStays: false);
                go.transform.localPosition = new Vector3(xOffset, startY + i * spacing, 0f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = JackSortingOrder;
                var collider = go.AddComponent<CircleCollider2D>();
                // Sprite is 1×1 in local units; world radius = 0.5 * localScale. Setting collider
                // radius to 0.5 keeps the click area matched to the visible circle.
                collider.radius = 0.5f;
                collider.isTrigger = true;

                var jack = go.AddComponent<JackView>();
                jack.Initialize(NodeId, slotIndex: i, isInput: isInput);
                go.transform.localScale = new Vector3(jackRadius * 2f, jackRadius * 2f, 1f);
                bucket.Add(jack);
            }
        }

        private static Sprite _bodyFallback;
        private static Sprite RoundedSquare()
        {
            if (_bodyFallback != null) return _bodyFallback;
            var tex = Texture2D.whiteTexture;
            _bodyFallback = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            _bodyFallback.name = "NodeBody";
            return _bodyFallback;
        }
    }
}
