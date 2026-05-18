using UnityEngine;

namespace ShapeConnections.Game.Wiring
{
    /// <summary>
    /// Visual patch cable between two endpoints. Renders a quadratic bezier
    /// with downward sag proportional to horizontal distance — gives a synth-
    /// modular feel. While dragging, the "to" endpoint can be set to a free
    /// cursor position via <see cref="SetEndpointTo"/>.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class WireView : MonoBehaviour
    {
        [SerializeField, Range(2, 64)] private int segments = 24;
        [SerializeField] private float sagPerUnit = 0.18f;
        [SerializeField] private float minSag = 0.08f;
        [SerializeField] private float width = 0.06f;
        [SerializeField] private Color color = new Color(0.95f, 0.5f, 0.2f, 1f);

        private LineRenderer _line;
        private Vector3 _from;
        private Vector3 _to;

        /// <summary>Source jack (non-null on a confirmed wire; null while still dragging away from a jack).</summary>
        public JackView From { get; private set; }

        /// <summary>Target jack (null while drag is pending).</summary>
        public JackView To { get; private set; }

        public bool IsPending => To == null;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.positionCount = segments;
            _line.startWidth = _line.endWidth = width;
            _line.material = new Material(Shader.Find("Sprites/Default"));
            _line.startColor = _line.endColor = color;
            _line.useWorldSpace = true;
            _line.sortingOrder = 15; // top of the visual stack: above cells/body/jacks
        }

        public void SetEndpointFrom(JackView jack)
        {
            From = jack;
            _from = jack.WorldPosition;
            Redraw();
        }

        public void SetEndpointTo(JackView jack)
        {
            To = jack;
            _to = jack.WorldPosition;
            Redraw();
        }

        /// <summary>Used during drag to follow the cursor while no target jack is hovered.</summary>
        public void SetEndpointTo(Vector3 worldPos)
        {
            To = null;
            _to = worldPos;
            Redraw();
        }

        public void RefreshFromJacks()
        {
            if (From != null) _from = From.WorldPosition;
            if (To != null)   _to   = To.WorldPosition;
            Redraw();
        }

        private void LateUpdate()
        {
            // Keep endpoints aligned if the underlying jacks move.
            if (From != null && To != null) RefreshFromJacks();
        }

        private void Redraw()
        {
            if (_line == null) return;
            var mid = (_from + _to) * 0.5f;
            var horizDist = Mathf.Abs(_from.x - _to.x);
            var sag = Mathf.Max(minSag, horizDist * sagPerUnit);
            var ctrl = mid + Vector3.down * sag;

            for (var i = 0; i < segments; i++)
            {
                var t = i / (float)(segments - 1);
                var omt = 1f - t;
                var p = omt * omt * _from + 2f * omt * t * ctrl + t * t * _to;
                _line.SetPosition(i, p);
            }
        }
    }
}
