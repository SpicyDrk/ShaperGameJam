using System.Collections.Generic;

namespace ShapeConnections.Simulation.GameLoop
{
    /// <summary>
    /// Outcome of one run of the core loop: the computed outputs at each level
    /// output socket, whether the win condition is satisfied, and any validation
    /// failures (cycle, unwired) reported by the evaluator.
    /// </summary>
    public sealed class LoopResult
    {
        public IReadOnlyList<Shape> ComputedOutputs { get; }
        public bool Win { get; }
        public bool HasCycle { get; }
        public IReadOnlyCollection<string> CycleNodeIds { get; }
        public IReadOnlyCollection<int> UnwiredOutputSockets { get; }

        public LoopResult(
            IReadOnlyList<Shape> computedOutputs,
            bool win,
            bool hasCycle,
            IReadOnlyCollection<string> cycleNodeIds,
            IReadOnlyCollection<int> unwiredOutputSockets)
        {
            ComputedOutputs = computedOutputs;
            Win = win;
            HasCycle = hasCycle;
            CycleNodeIds = cycleNodeIds;
            UnwiredOutputSockets = unwiredOutputSockets;
        }
    }
}
