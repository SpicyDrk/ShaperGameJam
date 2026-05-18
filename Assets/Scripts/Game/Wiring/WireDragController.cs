using System.Collections.Generic;
using ShapeConnections.Game.Input;
using ShapeConnections.Simulation.GameLoop;
using UnityEngine;

namespace ShapeConnections.Game.Wiring
{
    /// <summary>
    /// Mediates pointer input against <see cref="JackView"/>s in the scene.
    /// On press over a jack: spawn a pending <see cref="WireView"/> anchored to
    /// it. On release over another jack: confirm. On release over empty space:
    /// destroy the pending wire.
    /// </summary>
    /// <remarks>
    /// Driven by an <see cref="IPointerSource"/> rather than Unity's OnMouse*
    /// messages so tests / touch / gamepad can swap in without touching this class.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class WireDragController : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private GameObject wirePrefab;
        [Tooltip("If true, completed wires that target the same input jack twice replace the existing wire instead of stacking.")]
        [SerializeField] private bool replaceDuplicateInputs = true;

        private IPointerSource _pointer;
        private readonly List<WireView> _wires = new List<WireView>();
        private WireView _pending;
        private JackView _hovered;

        public IReadOnlyList<WireView> Wires => _wires;
        public event System.Action<WireView> WireCompleted;
        public event System.Action<WireView> WireRemoved;

        public void Initialize(IPointerSource pointer)
        {
            _pointer = pointer;
            if (worldCamera == null) worldCamera = Camera.main;
        }

        private void Awake()
        {
            if (_pointer == null) _pointer = new MousePointerSource();
            if (worldCamera == null) worldCamera = Camera.main;
        }

        private void Update()
        {
            if (worldCamera == null) return;
            var (sx, sy) = _pointer.ScreenPosition;
            var screen = new Vector3(sx, sy, 0f);
            var world = worldCamera.ScreenToWorldPoint(screen);
            world.z = 0f;

            UpdateHover(world);

            if (_pointer.WasPressedThisFrame && _hovered != null)
            {
                BeginDrag(_hovered);
            }

            if (_pending != null)
            {
                if (_hovered != null) _pending.SetEndpointTo(_hovered.WorldPosition);
                else                  _pending.SetEndpointTo(world);
            }

            if (_pointer.WasReleasedThisFrame && _pending != null)
            {
                EndDrag(_hovered);
            }
        }

        private void UpdateHover(Vector3 world)
        {
            var hit = Physics2D.OverlapPoint(new Vector2(world.x, world.y));
            JackView jack = null;
            if (hit != null) jack = hit.GetComponent<JackView>();

            if (jack == _hovered) return;
            if (_hovered != null) _hovered.RaiseUnhovered();
            _hovered = jack;
            if (_hovered != null) _hovered.RaiseHovered();
        }

        private void BeginDrag(JackView source)
        {
            source.RaisePressed();
            var go = wirePrefab != null
                ? Instantiate(wirePrefab, transform)
                : new GameObject("Wire");
            if (go.transform.parent != transform) go.transform.SetParent(transform, worldPositionStays: false);

            var wire = go.GetComponent<WireView>() ?? go.AddComponent<WireView>();
            if (go.GetComponent<LineRenderer>() == null) go.AddComponent<LineRenderer>();
            wire.SetEndpointFrom(source);
            wire.SetEndpointTo(source.WorldPosition);
            _pending = wire;
        }

        private void EndDrag(JackView target)
        {
            if (_pending == null) return;
            var wire = _pending;
            _pending = null;

            if (target == null || target == wire.From)
            {
                Destroy(wire.gameObject);
                return;
            }

            // Direction sanity: must be output → input.
            var from = wire.From;
            JackView outJack, inJack;
            if (!from.IsInput && target.IsInput) { outJack = from;   inJack = target; }
            else if (from.IsInput && !target.IsInput) { outJack = target; inJack = from;   }
            else
            {
                // Both input-input or output-output — invalid.
                Destroy(wire.gameObject);
                return;
            }

            // Self-loop sanity: a node's own output cannot feed its own input.
            // The evaluator would catch this as a cycle, but rejecting upfront is clearer UX.
            if (outJack.NodeId == inJack.NodeId)
            {
                Destroy(wire.gameObject);
                return;
            }

            if (replaceDuplicateInputs)
            {
                for (var i = _wires.Count - 1; i >= 0; i--)
                {
                    var existing = _wires[i];
                    if (existing.To != null && existing.To == inJack)
                    {
                        _wires.RemoveAt(i);
                        WireRemoved?.Invoke(existing);
                        Destroy(existing.gameObject);
                    }
                }
            }

            wire.SetEndpointFrom(outJack);
            wire.SetEndpointTo(inJack);
            target.RaiseReleased();
            _wires.Add(wire);
            WireCompleted?.Invoke(wire);
        }
    }
}
