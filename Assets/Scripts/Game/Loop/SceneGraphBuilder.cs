using System.Collections.Generic;
using ShapeConnections.Data;
using ShapeConnections.Game.Wiring;
using ShapeConnections.Simulation;
using ShapeConnections.Simulation.GameLoop;
using UnityEngine;

namespace ShapeConnections.Game.Loop
{
    /// <summary>
    /// IGraphBuilder backed by the live scene: walks every <see cref="NodeView"/>
    /// and <see cref="WireView"/> and assembles a <see cref="Graph"/>. Distinct
    /// from the runner so a test or replay system can swap in a different builder
    /// without touching the rest of the loop.
    /// </summary>
    public sealed class SceneGraphBuilder : IGraphBuilder
    {
        private readonly LevelDefinition _level;
        private readonly Transform _nodesRoot;
        private readonly WireDragController _wires;
        private readonly Dictionary<string, JackView> _inputSocketsByNodeId = new Dictionary<string, JackView>();
        private readonly Dictionary<string, JackView> _outputSocketsByNodeId = new Dictionary<string, JackView>();

        public SceneGraphBuilder(
            LevelDefinition level,
            Transform nodesRoot,
            WireDragController wires,
            IReadOnlyDictionary<int, JackView> inputSockets,
            IReadOnlyDictionary<int, JackView> outputSockets)
        {
            _level = level;
            _nodesRoot = nodesRoot;
            _wires = wires;

            foreach (var kvp in inputSockets)
            {
                _inputSocketsByNodeId[kvp.Value.NodeId] = kvp.Value;
            }
            foreach (var kvp in outputSockets)
            {
                _outputSocketsByNodeId[kvp.Value.NodeId] = kvp.Value;
            }
        }

        public IReadOnlyList<Shape> InputShapes
        {
            get
            {
                var list = new Shape[_level.Inputs.Count];
                for (var i = 0; i < _level.Inputs.Count; i++)
                {
                    list[i] = _level.Inputs[i] != null ? _level.Inputs[i].ToShape() : default;
                }
                return list;
            }
        }

        public int OutputSocketCount => _level.Targets.Count;

        public Graph Build()
        {
            var graph = new Graph();

            // 1. Collect placed NodeViews under the nodes root.
            var placed = _nodesRoot.GetComponentsInChildren<NodeView>();
            foreach (var view in placed)
            {
                if (view.Definition == null) continue;
                graph.AddNode(view.Definition.CreateNode(view.NodeId));
            }

            // 2. Translate WireViews into Connections.
            foreach (var wire in _wires.Wires)
            {
                if (wire.From == null || wire.To == null) continue;

                var fromId = TranslateNodeId(wire.From, isSource: true);
                var toId   = TranslateNodeId(wire.To,   isSource: false);

                if (fromId == null || toId == null) continue;

                graph.AddConnection(new Connection(
                    fromNodeId: fromId,
                    fromOutputIndex: wire.From.SlotIndex,
                    toNodeId: toId,
                    toInputIndex: wire.To.SlotIndex));
            }

            return graph;
        }

        private string TranslateNodeId(JackView jack, bool isSource)
        {
            // Edge sockets translate to the synthetic source/sink reserved ids.
            if (_inputSocketsByNodeId.ContainsKey(jack.NodeId))  return Graph.SourceNodeId;
            if (_outputSocketsByNodeId.ContainsKey(jack.NodeId)) return Graph.SinkNodeId;
            return jack.NodeId;
        }
    }
}
