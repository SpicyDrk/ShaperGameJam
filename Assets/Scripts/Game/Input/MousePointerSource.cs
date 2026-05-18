using ShapeConnections.Simulation.GameLoop;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShapeConnections.Game.Input
{
    /// <summary>
    /// IPointerSource backed by Unity's new Input System mouse device. Falls back
    /// silently when no mouse is connected (returns zero / not-pressed).
    /// </summary>
    public sealed class MousePointerSource : IPointerSource
    {
        public (float x, float y) ScreenPosition
        {
            get
            {
                var m = Mouse.current;
                if (m == null) return (0f, 0f);
                var p = m.position.ReadValue();
                return (p.x, p.y);
            }
        }

        public bool IsPressed => Mouse.current?.leftButton.isPressed ?? false;
        public bool WasPressedThisFrame => Mouse.current?.leftButton.wasPressedThisFrame ?? false;
        public bool WasReleasedThisFrame => Mouse.current?.leftButton.wasReleasedThisFrame ?? false;
    }
}
