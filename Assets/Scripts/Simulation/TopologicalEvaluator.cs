using System;
using System.Collections.Generic;
using System.Linq;

namespace ShapeConnections.Simulation
{
    /// <summary>
    /// Evaluates a <see cref="Graph"/> by topologically ordering its nodes
    /// (Kahn's algorithm) and computing each node's outputs from its incoming
    /// connections. Evaluation is instant and synchronous: matches the
    /// "wires between boxes" interaction model rather than tick-based flow.
    /// </summary>
    public sealed class TopologicalEvaluator
    {
        public EvaluationResult Evaluate(Graph graph, IReadOnlyList<Shape> inputShapes, int outputSocketCount)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (inputShapes == null) throw new ArgumentNullException(nameof(inputShapes));
            if (outputSocketCount < 0) throw new ArgumentOutOfRangeException(nameof(outputSocketCount));

            var nodes = graph.Nodes.ToList();
            var connections = graph.Connections;

            // 1. Build per-node in-degree (counting only edges from real nodes).
            //    Source-originated edges don't increase in-degree because the
            //    source is already "resolved" before evaluation starts.
            var inDegree = nodes.ToDictionary(n => n.Id, _ => 0);
            foreach (var conn in connections)
            {
                if (conn.FromNodeId == Graph.SourceNodeId) continue;
                if (conn.ToNodeId == Graph.SinkNodeId) continue;
                if (inDegree.ContainsKey(conn.ToNodeId))
                {
                    inDegree[conn.ToNodeId]++;
                }
            }

            // 2. Kahn's algorithm. Seed with nodes whose only inputs come from the source
            //    (or which have no inputs at all).
            var ready = new Queue<Node>(nodes.Where(n => inDegree[n.Id] == 0));
            var order = new List<Node>(nodes.Count);

            while (ready.Count > 0)
            {
                var node = ready.Dequeue();
                order.Add(node);

                foreach (var conn in graph.GetOutgoingConnections(node.Id))
                {
                    if (conn.ToNodeId == Graph.SinkNodeId) continue;
                    if (!inDegree.ContainsKey(conn.ToNodeId)) continue;
                    inDegree[conn.ToNodeId]--;
                    if (inDegree[conn.ToNodeId] == 0 && graph.TryGetNode(conn.ToNodeId, out var next))
                    {
                        ready.Enqueue(next);
                    }
                }
            }

            // 3. Cycle detection.
            if (order.Count < nodes.Count)
            {
                var cycleNodes = nodes
                    .Where(n => !order.Contains(n))
                    .Select(n => n.Id)
                    .ToArray();

                var emptyOutputs = new Shape[outputSocketCount];
                var unwired = Enumerable.Range(0, outputSocketCount).ToArray();
                return new EvaluationResult(
                    outputs: emptyOutputs,
                    hasCycle: true,
                    cycleNodeIds: cycleNodes,
                    unwiredOutputSockets: unwired);
            }

            // 4. Compute outputs for each node in order.
            var nodeOutputs = new Dictionary<(string nodeId, int slot), Shape>();
            foreach (var node in order)
            {
                var inputs = new Shape[node.InputSlotCount];
                for (var slot = 0; slot < node.InputSlotCount; slot++)
                {
                    inputs[slot] = ResolveInputShape(graph, node.Id, slot, inputShapes, nodeOutputs);
                }

                var outputs = node.Process(inputs);
                if (outputs == null || outputs.Count != node.OutputSlotCount)
                {
                    throw new InvalidOperationException(
                        $"Node '{node.Id}' returned {outputs?.Count ?? 0} outputs but declares {node.OutputSlotCount}.");
                }

                for (var slot = 0; slot < node.OutputSlotCount; slot++)
                {
                    nodeOutputs[(node.Id, slot)] = outputs[slot];
                }
            }

            // 5. Resolve shapes at the sink output sockets.
            var sinkOutputs = new Shape[outputSocketCount];
            var unwiredSinks = new List<int>();
            for (var socket = 0; socket < outputSocketCount; socket++)
            {
                var feeder = connections.FirstOrDefault(c => c.ToNodeId == Graph.SinkNodeId && c.ToInputIndex == socket);
                if (feeder.ToNodeId != Graph.SinkNodeId)
                {
                    // FirstOrDefault on a struct returns default — recognise the "no feeder" case.
                    unwiredSinks.Add(socket);
                    continue;
                }

                if (feeder.FromNodeId == Graph.SourceNodeId)
                {
                    sinkOutputs[socket] = feeder.FromOutputIndex < inputShapes.Count
                        ? inputShapes[feeder.FromOutputIndex]
                        : default;
                }
                else if (nodeOutputs.TryGetValue((feeder.FromNodeId, feeder.FromOutputIndex), out var s))
                {
                    sinkOutputs[socket] = s;
                }
                else
                {
                    unwiredSinks.Add(socket);
                }
            }

            return new EvaluationResult(
                outputs: sinkOutputs,
                hasCycle: false,
                cycleNodeIds: Array.Empty<string>(),
                unwiredOutputSockets: unwiredSinks);
        }

        private static Shape ResolveInputShape(
            Graph graph,
            string nodeId,
            int slot,
            IReadOnlyList<Shape> inputShapes,
            Dictionary<(string, int), Shape> nodeOutputs)
        {
            var feeder = graph.GetIncomingConnections(nodeId).FirstOrDefault(c => c.ToInputIndex == slot);
            if (feeder.ToNodeId != nodeId)
            {
                // No incoming connection on this slot — treat as default shape. UI flags it
                // separately via ValidationResult (handled in Phase 6).
                return default;
            }

            if (feeder.FromNodeId == Graph.SourceNodeId)
            {
                return feeder.FromOutputIndex < inputShapes.Count
                    ? inputShapes[feeder.FromOutputIndex]
                    : default;
            }

            return nodeOutputs.TryGetValue((feeder.FromNodeId, feeder.FromOutputIndex), out var s) ? s : default;
        }
    }
}
