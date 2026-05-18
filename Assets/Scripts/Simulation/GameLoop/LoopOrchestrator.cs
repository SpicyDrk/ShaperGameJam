using System;
using System.Collections.Generic;

namespace ShapeConnections.Simulation.GameLoop
{
    /// <summary>
    /// Coordinates one tick of the core game loop: ask the graph builder for the
    /// current graph, evaluate it, ask the win condition whether targets were met.
    /// Pure C# — no Unity dependency. EditMode-testable with stub IGraphBuilder /
    /// IWinCondition implementations.
    /// </summary>
    /// <remarks>
    /// This is the central seam for "make the core loop modifiable" — every
    /// dependency is an interface, so individual aspects (graph source, win
    /// rule, evaluator implementation) can be swapped without touching the
    /// orchestrator itself.
    /// </remarks>
    public sealed class LoopOrchestrator
    {
        private readonly IGraphBuilder _builder;
        private readonly IWinCondition _winCondition;
        private readonly TopologicalEvaluator _evaluator;

        public LoopOrchestrator(IGraphBuilder builder, IWinCondition winCondition, TopologicalEvaluator evaluator = null)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _winCondition = winCondition ?? throw new ArgumentNullException(nameof(winCondition));
            _evaluator = evaluator ?? new TopologicalEvaluator();
        }

        public LoopResult Run(IReadOnlyList<Shape> targetOutputs)
        {
            if (targetOutputs == null) throw new ArgumentNullException(nameof(targetOutputs));

            var graph = _builder.Build();
            var evaluation = _evaluator.Evaluate(graph, _builder.InputShapes, _builder.OutputSocketCount);

            // Win requires: no cycle, every output socket actually fed by a wire, AND
            // the win condition satisfied. The unwired check matters because the
            // evaluator fills unwired sockets with default(Shape) — which happens to
            // be Square+None and would silently match a Square+None target.
            var win = !evaluation.HasCycle
                   && evaluation.UnwiredOutputSockets.Count == 0
                   && _winCondition.IsSatisfied(evaluation.Outputs, targetOutputs);

            return new LoopResult(
                computedOutputs: evaluation.Outputs,
                win: win,
                hasCycle: evaluation.HasCycle,
                cycleNodeIds: evaluation.CycleNodeIds,
                unwiredOutputSockets: evaluation.UnwiredOutputSockets);
        }
    }
}
