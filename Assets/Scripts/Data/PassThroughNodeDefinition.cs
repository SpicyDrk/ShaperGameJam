using ShapeConnections.Simulation;
using UnityEngine;

namespace ShapeConnections.Data
{
    [CreateAssetMenu(menuName = "Shape Connections/Nodes/Pass-Through", fileName = "PassThrough")]
    public sealed class PassThroughNodeDefinition : NodeDefinition
    {
        public override int InputSlotCount => 1;
        public override int OutputSlotCount => 1;

        public override Node CreateNode(string id) => new PassThroughNode(id);
    }
}
