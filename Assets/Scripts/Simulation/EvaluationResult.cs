using System.Collections.Generic;

namespace ShapeConnections.Simulation
{
    /// <summary>
    /// Outcome of a <see cref="TopologicalEvaluator.Evaluate"/> call.
    /// <para>
    /// When <see cref="HasCycle"/> is true, <see cref="Outputs"/> contains
    /// <see cref="default(Shape)"/> entries for every output socket and
    /// <see cref="CycleNodeIds"/> lists the nodes participating in (or downstream
    /// of) the cycle. UI validation should use this to mark wires in red and
    /// disable the Run button.
    /// </para>
    /// </summary>
    public sealed class EvaluationResult
    {
        public IReadOnlyList<Shape> Outputs { get; }
        public bool HasCycle { get; }
        public IReadOnlyCollection<string> CycleNodeIds { get; }
        public IReadOnlyCollection<int> UnwiredOutputSockets { get; }

        public EvaluationResult(
            IReadOnlyList<Shape> outputs,
            bool hasCycle,
            IReadOnlyCollection<string> cycleNodeIds,
            IReadOnlyCollection<int> unwiredOutputSockets)
        {
            Outputs = outputs;
            HasCycle = hasCycle;
            CycleNodeIds = cycleNodeIds;
            UnwiredOutputSockets = unwiredOutputSockets;
        }
    }
}
