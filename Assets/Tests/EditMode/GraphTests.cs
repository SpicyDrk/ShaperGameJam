using System.Linq;
using NUnit.Framework;
using ShapeConnections.Simulation;

namespace ShapeConnections.Tests
{
    public class GraphTests
    {
        private sealed class StubNode : Node
        {
            public StubNode(string id, int inputs, int outputs) : base(id, inputs, outputs) { }

            public override System.Collections.Generic.IReadOnlyList<Shape> Process(
                System.Collections.Generic.IReadOnlyList<Shape> inputs)
                => inputs;
        }

        private static StubNode MakeNode(string id = "n1", int inputs = 1, int outputs = 1)
            => new StubNode(id, inputs, outputs);

        [Test]
        public void NewGraph_HasNoNodesOrConnections()
        {
            var graph = new Graph();

            Assert.That(graph.Nodes, Is.Empty);
            Assert.That(graph.Connections, Is.Empty);
        }

        [Test]
        public void AddNode_AppearsInNodes()
        {
            var graph = new Graph();
            var node = MakeNode("a");

            graph.AddNode(node);

            Assert.That(graph.Nodes, Contains.Item(node));
            Assert.That(graph.TryGetNode("a", out var found), Is.True);
            Assert.That(found, Is.SameAs(node));
        }

        [Test]
        public void AddNode_DuplicateId_Throws()
        {
            var graph = new Graph();
            graph.AddNode(MakeNode("a"));

            Assert.Throws<System.InvalidOperationException>(() => graph.AddNode(MakeNode("a")));
        }

        [Test]
        public void AddConnection_AppearsInConnections()
        {
            var graph = new Graph();
            graph.AddNode(MakeNode("a", 0, 1));
            graph.AddNode(MakeNode("b", 1, 0));

            var conn = new Connection("a", 0, "b", 0);
            graph.AddConnection(conn);

            Assert.That(graph.Connections, Contains.Item(conn));
        }

        [Test]
        public void AddConnection_DuplicateInputSlot_Throws()
        {
            var graph = new Graph();
            graph.AddNode(MakeNode("a", 0, 1));
            graph.AddNode(MakeNode("b", 0, 1));
            graph.AddNode(MakeNode("c", 1, 0));
            graph.AddConnection(new Connection("a", 0, "c", 0));

            Assert.Throws<System.InvalidOperationException>(
                () => graph.AddConnection(new Connection("b", 0, "c", 0)));
        }

        [Test]
        public void GetIncoming_ReturnsConnectionsTargetingNode()
        {
            var graph = new Graph();
            graph.AddNode(MakeNode("a", 0, 1));
            graph.AddNode(MakeNode("b", 0, 1));
            graph.AddNode(MakeNode("c", 2, 1));
            graph.AddConnection(new Connection("a", 0, "c", 0));
            graph.AddConnection(new Connection("b", 0, "c", 1));

            var incoming = graph.GetIncomingConnections("c").ToList();

            Assert.That(incoming.Count, Is.EqualTo(2));
            Assert.That(incoming.Any(c => c.FromNodeId == "a" && c.ToInputIndex == 0), Is.True);
            Assert.That(incoming.Any(c => c.FromNodeId == "b" && c.ToInputIndex == 1), Is.True);
        }

        [Test]
        public void GetOutgoing_ReturnsConnectionsFromNode()
        {
            var graph = new Graph();
            graph.AddNode(MakeNode("a", 0, 1));
            graph.AddNode(MakeNode("b", 1, 0));
            graph.AddNode(MakeNode("c", 1, 0));
            graph.AddConnection(new Connection("a", 0, "b", 0));
            graph.AddConnection(new Connection("a", 0, "c", 0));

            var outgoing = graph.GetOutgoingConnections("a").ToList();

            Assert.That(outgoing.Count, Is.EqualTo(2));
        }

        [Test]
        public void RemoveNode_RemovesIncidentConnections()
        {
            var graph = new Graph();
            graph.AddNode(MakeNode("a", 0, 1));
            graph.AddNode(MakeNode("b", 1, 0));
            graph.AddConnection(new Connection("a", 0, "b", 0));

            graph.RemoveNode("a");

            Assert.That(graph.Nodes.Any(n => n.Id == "a"), Is.False);
            Assert.That(graph.Connections, Is.Empty);
        }

        [Test]
        public void AddConnection_FromMissingNode_Throws()
        {
            var graph = new Graph();
            graph.AddNode(MakeNode("b", 1, 0));

            Assert.Throws<System.InvalidOperationException>(
                () => graph.AddConnection(new Connection("ghost", 0, "b", 0)));
        }

        [Test]
        public void AddConnection_OutOfRangeSlot_Throws()
        {
            var graph = new Graph();
            graph.AddNode(MakeNode("a", 0, 1));
            graph.AddNode(MakeNode("b", 1, 0));

            // a has only output index 0
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => graph.AddConnection(new Connection("a", 1, "b", 0)));
        }
    }
}
