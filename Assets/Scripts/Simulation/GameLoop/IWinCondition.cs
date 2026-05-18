using System.Collections.Generic;

namespace ShapeConnections.Simulation.GameLoop
{
    /// <summary>
    /// Decides whether the computed outputs of a level pass the "you win" bar.
    /// The default implementation is exact positional match; future modes could
    /// score partial credit, golf-style minimum-nodes, or order-insensitive sets.
    /// </summary>
    public interface IWinCondition
    {
        bool IsSatisfied(IReadOnlyList<Shape> computedOutputs, IReadOnlyList<Shape> targetOutputs);
    }
}
