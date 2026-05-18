using System.Collections.Generic;
using NUnit.Framework;
using ShapeConnections.Simulation;
using ShapeConnections.Simulation.GameLoop;

namespace ShapeConnections.Tests
{
    public class ExactMatchWinConditionTests
    {
        [Test]
        public void DelegatesToTargetComparator_PassCase()
        {
            var cond = new ExactMatchWinCondition();
            var computed = new[] { new Shape(ShapeKind.Square, ShapeColor.Red) };
            var target   = new[] { new Shape(ShapeKind.Square, ShapeColor.Red) };

            Assert.That(cond.IsSatisfied(computed, target), Is.True);
        }

        [Test]
        public void DelegatesToTargetComparator_FailCase()
        {
            var cond = new ExactMatchWinCondition();
            var computed = new[] { new Shape(ShapeKind.Square) };
            var target   = new[] { new Shape(ShapeKind.Triangle) };

            Assert.That(cond.IsSatisfied(computed, target), Is.False);
        }

        [Test]
        public void RespectsWildcardColor()
        {
            var cond = new ExactMatchWinCondition();
            var computed = new[] { new Shape(ShapeKind.Square, ShapeColor.Red) };
            var target   = new[] { new Shape(ShapeKind.Square, ShapeColor.None) };

            Assert.That(cond.IsSatisfied(computed, target), Is.True);
        }
    }

    public class LoopOrchestratorTests
    {
        private sealed class StubGraphBuilder : IGraphBuilder
        {
            public Graph GraphToReturn { get; set; } = new Graph();
            public IReadOnlyList<Shape> InputShapes { get; set; } = System.Array.Empty<Shape>();
            public int OutputSocketCount { get; set; } = 1;
            public int BuildCalls { get; private set; }
            public Graph Build() { BuildCalls++; return GraphToReturn; }
        }

        private sealed class StubWinCondition : IWinCondition
        {
            public bool Result { get; set; }
            public int Calls { get; private set; }
            public bool IsSatisfied(IReadOnlyList<Shape> computed, IReadOnlyList<Shape> target)
            { Calls++; return Result; }
        }

        [Test]
        public void Run_BuildsGraphAndAsksWinCondition()
        {
            // Graph: Source[0] → PT → Sink[0]. Input Square should arrive at sink.
            var graph = new Graph();
            graph.AddNode(new PassThroughNode("pt"));
            graph.AddConnection(new Connection(Graph.SourceNodeId, 0, "pt", 0));
            graph.AddConnection(new Connection("pt", 0, Graph.SinkNodeId, 0));

            var builder = new StubGraphBuilder
            {
                GraphToReturn = graph,
                InputShapes = new[] { new Shape(ShapeKind.Square) },
                OutputSocketCount = 1
            };
            var win = new StubWinCondition { Result = true };

            var orch = new LoopOrchestrator(builder, win);
            var result = orch.Run(new[] { new Shape(ShapeKind.Square) });

            Assert.That(builder.BuildCalls, Is.EqualTo(1));
            Assert.That(win.Calls, Is.EqualTo(1));
            Assert.That(result.ComputedOutputs.Count, Is.EqualTo(1));
            Assert.That(result.ComputedOutputs[0], Is.EqualTo(new Shape(ShapeKind.Square)));
            Assert.That(result.Win, Is.True);
            Assert.That(result.HasCycle, Is.False);
        }

        [Test]
        public void Run_WhenCycle_WinIsFalseAndConditionNotCalled()
        {
            var graph = new Graph();
            graph.AddNode(new PassThroughNode("a"));
            graph.AddNode(new PassThroughNode("b"));
            graph.AddConnection(new Connection("a", 0, "b", 0));
            graph.AddConnection(new Connection("b", 0, "a", 0));

            var builder = new StubGraphBuilder { GraphToReturn = graph, OutputSocketCount = 0 };
            var win = new StubWinCondition { Result = true };

            var orch = new LoopOrchestrator(builder, win);
            var result = orch.Run(System.Array.Empty<Shape>());

            Assert.That(result.HasCycle, Is.True);
            Assert.That(result.Win, Is.False);
            Assert.That(win.Calls, Is.EqualTo(0), "Win condition must not run on a broken graph");
        }

        [Test]
        public void Run_WhenWinConditionRejects_WinIsFalse()
        {
            var builder = new StubGraphBuilder { OutputSocketCount = 1 };
            var win = new StubWinCondition { Result = false };

            var orch = new LoopOrchestrator(builder, win);
            var result = orch.Run(new[] { new Shape(ShapeKind.Triangle) });

            Assert.That(result.HasCycle, Is.False);
            Assert.That(result.Win, Is.False);
        }

        // Gap #1 regression: when no wires reach the output sink, the evaluator
        // returns default(Shape) at each socket. The default happens to be Square+None,
        // which would coincidentally pass a Square+None target unless the orchestrator
        // explicitly rejects unwired-output cases. Win condition must NOT be consulted.
        [Test]
        public void Run_WithUnwiredOutputSockets_WinIsFalse_AndWinConditionNotCalled()
        {
            var builder = new StubGraphBuilder
            {
                GraphToReturn = new Graph(),         // no nodes, no connections
                InputShapes = new[] { new Shape(ShapeKind.Square) },
                OutputSocketCount = 1
            };
            var win = new StubWinCondition { Result = true };

            var orch = new LoopOrchestrator(builder, win);
            var result = orch.Run(new[] { new Shape(ShapeKind.Square) }); // target matches default Shape

            Assert.That(result.HasCycle, Is.False);
            Assert.That(result.UnwiredOutputSockets, Is.Not.Empty);
            Assert.That(result.Win, Is.False, "Unwired outputs must never count as a win, even when the default Shape happens to match the target.");
            Assert.That(win.Calls, Is.EqualTo(0), "Win condition must be short-circuited when outputs are unwired.");
        }
    }
}
