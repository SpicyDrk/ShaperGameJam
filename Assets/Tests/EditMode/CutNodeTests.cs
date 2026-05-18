using System.Collections.Generic;
using NUnit.Framework;
using ShapeConnections.Simulation;

namespace ShapeConnections.Tests
{
    public class CutNodeTests
    {
        private static readonly Shape Square   = new Shape(ShapeKind.Square);
        private static readonly Shape Triangle = new Shape(ShapeKind.Triangle);
        private static readonly Shape Circle   = new Shape(ShapeKind.Circle);

        [Test]
        public void Construction_HasOneInputAndTwoOutputs()
        {
            var node = new CutNode("c1", input: Square, top: Triangle, bottom: Triangle);

            Assert.That(node.InputSlotCount, Is.EqualTo(1));
            Assert.That(node.OutputSlotCount, Is.EqualTo(2));
        }

        // SC-06: Cut node produces both authored outputs in the authored order.
        [Test]
        public void Cut_Square_Produces_TopAndBottom_AsAuthored()
        {
            var node = new CutNode("cut", input: Square, top: Triangle, bottom: Triangle);

            var outputs = node.Process(new List<Shape> { Square });

            Assert.That(outputs.Count, Is.EqualTo(2));
            Assert.That(outputs[0], Is.EqualTo(Triangle), "slot 0 must be the authored TOP output");
            Assert.That(outputs[1], Is.EqualTo(Triangle), "slot 1 must be the authored BOTTOM output");
        }

        [Test]
        public void Cut_PreservesOutputOrder_WhenTopAndBottomDiffer()
        {
            // Top vs bottom must not be swappable: a swap would silently break level wiring.
            var node = new CutNode("cut", input: Square, top: Triangle, bottom: Circle);

            var outputs = node.Process(new List<Shape> { Square });

            Assert.That(outputs[0], Is.EqualTo(Triangle));
            Assert.That(outputs[1], Is.EqualTo(Circle));
            Assert.That(outputs[0], Is.Not.EqualTo(outputs[1]));
        }

        [Test]
        public void Cut_UnauthoredInput_ReturnsDefaults()
        {
            // Documented fallback (mirrors Phase 4 SC-08 pattern for Combine): unauthored
            // input combinations yield default(Shape), letting downstream validation flag the mismatch.
            var node = new CutNode("cut", input: Square, top: Triangle, bottom: Triangle);

            var outputs = node.Process(new List<Shape> { Circle });

            Assert.That(outputs.Count, Is.EqualTo(2));
            Assert.That(outputs[0], Is.EqualTo(default(Shape)));
            Assert.That(outputs[1], Is.EqualTo(default(Shape)));
        }

        // Multi-downstream: a single Cut node feeding two distinct sinks must produce
        // the correct output at each sink slot.
        [Test]
        public void Cut_WithTwoDownstreamSinks_ProducesAuthoredOutputAtEachSink()
        {
            var graph = new Graph();
            graph.AddNode(new CutNode("cut", input: Square, top: Triangle, bottom: Triangle));
            graph.AddConnection(new Connection(Graph.SourceNodeId, 0, "cut", 0));
            graph.AddConnection(new Connection("cut", 0, Graph.SinkNodeId, 0));
            graph.AddConnection(new Connection("cut", 1, Graph.SinkNodeId, 1));

            var evaluator = new TopologicalEvaluator();
            var result = evaluator.Evaluate(graph, new[] { Square }, outputSocketCount: 2);

            Assert.That(result.HasCycle, Is.False);
            Assert.That(result.Outputs.Count, Is.EqualTo(2));
            Assert.That(result.Outputs[0], Is.EqualTo(Triangle));
            Assert.That(result.Outputs[1], Is.EqualTo(Triangle));
            Assert.That(result.UnwiredOutputSockets, Is.Empty);
        }
    }
}
