using System;
using UnityEngine;

namespace ShapeConnections.Game.Wiring
{
    /// <summary>
    /// Single input or output jack on a <see cref="NodeView"/>. Detects pointer
    /// input via a CircleCollider2D + Unity OnMouse* messages, and re-raises as
    /// plain C# events so the wiring layer can subscribe without coupling to
    /// Unity's input plumbing.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class JackView : MonoBehaviour
    {
        [SerializeField] private Color idleColor  = new Color(0.15f, 0.15f, 0.15f, 1f);
        [SerializeField] private Color hoverColor = new Color(1f,    0.85f, 0.2f,  1f);

        private SpriteRenderer _sprite;

        public string NodeId { get; private set; }
        public int SlotIndex { get; private set; }
        public bool IsInput { get; private set; }

        /// <summary>True while the pointer is hovering this jack.</summary>
        public bool IsHovered { get; private set; }

        public Vector3 WorldPosition => transform.position;

        public event Action<JackView> Pressed;
        public event Action<JackView> Released;
        public event Action<JackView> PointerEntered;
        public event Action<JackView> PointerExited;

        public void Initialize(string nodeId, int slotIndex, bool isInput)
        {
            NodeId = nodeId;
            SlotIndex = slotIndex;
            IsInput = isInput;
            name = $"Jack[{(isInput ? "in" : "out")}:{slotIndex}]";

            _sprite = GetComponent<SpriteRenderer>();
            if (_sprite.sprite == null) _sprite.sprite = BuiltinCircleSprite();
            _sprite.color = idleColor;
        }

        // Called by WireDragController — we don't use Unity's OnMouse* messages
        // because the new Input System disables them by default.
        internal void RaisePressed()  => Pressed?.Invoke(this);
        internal void RaiseReleased() => Released?.Invoke(this);

        internal void RaiseHovered()
        {
            if (IsHovered) return;
            IsHovered = true;
            if (_sprite != null) _sprite.color = hoverColor;
            PointerEntered?.Invoke(this);
        }

        internal void RaiseUnhovered()
        {
            if (!IsHovered) return;
            IsHovered = false;
            if (_sprite != null) _sprite.color = idleColor;
            PointerExited?.Invoke(this);
        }

        private static Sprite _fallback;
        private static Sprite BuiltinCircleSprite()
        {
            if (_fallback != null) return _fallback;
            // Generate a circle into a small white texture.
            const int s = 32;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            var pixels = new Color32[s * s];
            var r2 = (s * 0.5f) * (s * 0.5f);
            for (var y = 0; y < s; y++)
            for (var x = 0; x < s; x++)
            {
                var dx = x - s * 0.5f + 0.5f;
                var dy = y - s * 0.5f + 0.5f;
                pixels[y * s + x] = (dx * dx + dy * dy) <= r2 ? new Color32(255,255,255,255) : new Color32(0,0,0,0);
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            _fallback = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
            _fallback.name = "JackCircle";
            return _fallback;
        }
    }
}
