using System;

namespace ShapeConnections.Simulation.GameLoop
{
    /// <summary>
    /// Source of "the player wants to evaluate the graph now" signals.
    /// The default implementation is a UI Button; alternates could include a
    /// hotkey, an auto-runner, an AI bot, or a replay system.
    /// </summary>
    public interface IRunTrigger
    {
        event Action RunRequested;
    }
}
