using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ShapeConnections.Simulation;

namespace ShapeConnections.Tests
{
    public class TopologicalEvaluatorTests
    {
        /// <summary>
        /// Records its id into a shared list every time <see cref="Process"/> is called,
        /// so tests can assert evaluation order and call count.
        /// </summary>
        private sealed class RecordingPassThroughNode : Node
        {
            private readonly List<string> _log;

            public RecordingPassThroughNode(string id, List<string> log, int inputs = 1, int outputs = 1)
                : base(id, inputs, outputs)
            {
                _log = log;
            }

            public override IReadOnlyList<Shape> Process(IReadOnlyList<Shape> inputs)
            {
                _log.Add(Id);
                // Forward first input across all outputs (sufficient for ordering tests).
                var result = new Shape[OutputSlotCount];
                var seed = inputs.Count > 0 ? inputs[0] : default;
                for (var i = 0; i < OutputSlotCount; i++) result[i] = seed;
                return result;
            }
        }

        // SC-01: Pass-Through processes shapes through a graph.
        [Test]
        public void SC01_PassThrough_ForwardsShape_ReturnsSameShape()
        {
            var graph = new Graph();
            graph.AddNode(new PassThroughNode("pt"));
            graph.AddConnection(new Connection(Graph.SourceNodeId, 0, "pt", 0));
            graph.AddConnection(new Connection("pt", 0, Graph.SinkNodeId, 0));

            var input = new Shape(ShapeKind.Square, ShapeColor.Red);
            var evaluator = new TopologicalEvaluator();
            var result = evaluator.Evaluate(graph, new[] { input }, outputSocketCount: 1);

            Assert.That(result.HasCycle, Is.False);
            Assert.That(result.Outputs.Count, Is.EqualTo(1));
            Assert.That(result.Outputs[0], Is.EqualTo(input));
        }

        // SC-02: Topological evaluator respects node ordering.
        // Graph:  Source → A → B → C → Sink
        //                      D → C
        //         Source → D
        // Expect: each of A,B,C,D is called exactly once. A before B. B before C. D before C.
        [Test]
        public void SC02_Topological_MultiBranch_EvaluatesEachNodeOnce_InOrder()
        {
            var log = new List<string>();
            var graph = new Graph();
            graph.AddNode(new RecordingPassThroughNode("A", log));
            graph.AddNode(new RecordingPassThroughNode("B", log));
            graph.AddNode(new RecordingPassThroughNode("C", log, inputs: 2, outputs: 1));
            graph.AddNode(new RecordingPassThroughNode("D", log));

            graph.AddConnection(new Connection(Graph.SourceNodeId, 0, "A", 0));
            graph.AddConnection(new Connection(Graph.SourceNodeId, 1, "D", 0));
            graph.AddConnection(new Connection("A", 0, "B", 0));
            graph.AddConnection(new Connection("B", 0, "C", 0));
            graph.AddConnection(new Connection("D", 0, "C", 1));
            graph.AddConnection(new Connection("C", 0, Graph.SinkNodeId, 0));

            var evaluator = new TopologicalEvaluator();
            var result = evaluator.Evaluate(
                graph,
                new[] { new Shape(ShapeKind.Square), new Shape(ShapeKind.Triangle) },
                outputSocketCount: 1);

            Assert.That(result.HasCycle, Is.False);

            // Each node called exactly once.
            Assert.That(log.Count, Is.EqualTo(4));
            Assert.That(log.Distinct().Count(), Is.EqualTo(4));

            // Topological constraints.
            int IndexOf(string id) => log.IndexOf(id);
            Assert.That(IndexOf("A"), Is.LessThan(IndexOf("B")), "A must run before B");
            Assert.That(IndexOf("B"), Is.LessThan(IndexOf("C")), "B must run before C");
            Assert.That(IndexOf("D"), Is.LessThan(IndexOf("C")), "D must run before C");
        }

        [Test]
        public void Cycle_IsDetected_AndReportedInResult()
        {
            // A → B → A is a cycle; the evaluator should flag it rather than loop forever.
            var graph = new Graph();
            graph.AddNode(new PassThroughNode("A"));
            graph.AddNode(new PassThroughNode("B"));
            graph.AddConnection(new Connection("A", 0, "B", 0));
            graph.AddConnection(new Connection("B", 0, "A", 0));

            var evaluator = new TopologicalEvaluator();
            var result = evaluator.Evaluate(graph, System.Array.Empty<Shape>(), outputSocketCount: 0);

            Assert.That(result.HasCycle, Is.True);
            Assert.That(result.CycleNodeIds, Is.SupersetOf(new[] { "A", "B" }));
        }

        [Test]
        public void UnwiredOutputSocket_ReturnsDefaultShape()
        {
            // Sink socket 0 is unconnected — evaluator returns default(Shape) there
            // rather than throwing. UI validation will flag this to the player.
            var graph = new Graph();
            var evaluator = new TopologicalEvaluator();

            var result = evaluator.Evaluate(graph, System.Array.Empty<Shape>(), outputSocketCount: 1);

            Assert.That(result.HasCycle, Is.False);
            Assert.That(result.Outputs.Count, Is.EqualTo(1));
            Assert.That(result.Outputs[0], Is.EqualTo(default(Shape)));
            Assert.That(result.UnwiredOutputSockets, Contains.Item(0));
        }
    }
}
