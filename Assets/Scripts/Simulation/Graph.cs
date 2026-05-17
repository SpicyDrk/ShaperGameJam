using System;
using System.Collections.Generic;
using System.Linq;

namespace ShapeConnections.Simulation
{
    /// <summary>
    /// A graph of <see cref="Node"/>s wired together by <see cref="Connection"/>s.
    /// The graph also carries synthetic source / sink endpoints addressed by the
    /// reserved ids <see cref="SourceNodeId"/> and <see cref="SinkNodeId"/> — these
    /// represent the level's edge input shapes and target output sockets and are
    /// allocated by the <see cref="TopologicalEvaluator"/>, not added as
    /// <see cref="Node"/>s.
    /// </summary>
    public sealed class Graph
    {
        /// <summary>Reserved node id used by the evaluator to denote the level's input source.</summary>
        public const string SourceNodeId = "__source__";

        /// <summary>Reserved node id used by the evaluator to denote the level's output sink.</summary>
        public const string SinkNodeId = "__sink__";

        private readonly Dictionary<string, Node> _nodes = new Dictionary<string, Node>();
        private readonly List<Connection> _connections = new List<Connection>();

        public IReadOnlyCollection<Node> Nodes => _nodes.Values;
        public IReadOnlyList<Connection> Connections => _connections;

        public void AddNode(Node node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Id == SourceNodeId || node.Id == SinkNodeId)
            {
                throw new InvalidOperationException(
                    $"Node id '{node.Id}' is reserved for the level source/sink.");
            }

            if (_nodes.ContainsKey(node.Id))
            {
                throw new InvalidOperationException($"Node '{node.Id}' is already in the graph.");
            }

            _nodes.Add(node.Id, node);
        }

        public bool TryGetNode(string id, out Node node) => _nodes.TryGetValue(id, out node);

        public void RemoveNode(string id)
        {
            if (!_nodes.Remove(id)) return;
            _connections.RemoveAll(c => c.FromNodeId == id || c.ToNodeId == id);
        }

        public void AddConnection(Connection connection)
        {
            ValidateEndpoint(connection.FromNodeId, connection.FromOutputIndex, isOutput: true);
            ValidateEndpoint(connection.ToNodeId, connection.ToInputIndex, isOutput: false);

            // Each input slot may only have one incoming connection (last-write semantics rejected).
            if (_connections.Any(c => c.ToNodeId == connection.ToNodeId && c.ToInputIndex == connection.ToInputIndex))
            {
                throw new InvalidOperationException(
                    $"Input slot {connection.ToNodeId}[{connection.ToInputIndex}] is already wired.");
            }

            _connections.Add(connection);
        }

        public void RemoveConnection(Connection connection) => _connections.Remove(connection);

        public IEnumerable<Connection> GetIncomingConnections(string nodeId)
            => _connections.Where(c => c.ToNodeId == nodeId);

        public IEnumerable<Connection> GetOutgoingConnections(string nodeId)
            => _connections.Where(c => c.FromNodeId == nodeId);

        private void ValidateEndpoint(string nodeId, int slotIndex, bool isOutput)
        {
            // Source/Sink endpoints are validated by the evaluator, which knows the level's
            // socket counts. The graph permits any non-negative slot index for them.
            if (nodeId == SourceNodeId || nodeId == SinkNodeId)
            {
                if (slotIndex < 0) throw new ArgumentOutOfRangeException(nameof(slotIndex));
                return;
            }

            if (!_nodes.TryGetValue(nodeId, out var node))
            {
                throw new InvalidOperationException($"Node '{nodeId}' is not in the graph.");
            }

            var max = isOutput ? node.OutputSlotCount : node.InputSlotCount;
            if (slotIndex < 0 || slotIndex >= max)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slotIndex),
                    $"Slot index {slotIndex} is out of range for node '{nodeId}' (max {max - 1}).");
            }
        }
    }
}
