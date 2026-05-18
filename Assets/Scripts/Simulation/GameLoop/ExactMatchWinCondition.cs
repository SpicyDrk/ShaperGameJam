using System.Collections.Generic;

namespace ShapeConnections.Simulation.GameLoop
{
    /// <summary>Win condition: every computed output exactly matches its target via <see cref="TargetComparator"/>.</summary>
    public sealed class ExactMatchWinCondition : IWinCondition
    {
        public bool IsSatisfied(IReadOnlyList<Shape> computedOutputs, IReadOnlyList<Shape> targetOutputs)
            => TargetComparator.Matches(computedOutputs, targetOutputs);
    }
}
