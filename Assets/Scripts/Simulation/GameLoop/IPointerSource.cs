namespace ShapeConnections.Simulation.GameLoop
{
    /// <summary>
    /// Abstracted pointer state. Lets the drag controller stay agnostic of
    /// mouse vs touch vs synthetic-input-during-tests. Coordinates are screen
    /// pixels (origin bottom-left), consistent with Unity's input conventions.
    /// </summary>
    public interface IPointerSource
    {
        (float x, float y) ScreenPosition { get; }
        bool IsPressed { get; }
        bool WasPressedThisFrame { get; }
        bool WasReleasedThisFrame { get; }
    }
}
