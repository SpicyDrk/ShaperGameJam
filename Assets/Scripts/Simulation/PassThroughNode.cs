using System.Collections.Generic;

namespace ShapeConnections.Simulation
{
    /// <summary>
    /// Forwards its single input to its single output unchanged. The simplest
    /// node type — used to connect a shape across the grid without transformation.
    /// </summary>
    public sealed class PassThroughNode : Node
    {
        public PassThroughNode(string id) : base(id, inputSlotCount: 1, outputSlotCount: 1) { }

        public override IReadOnlyList<Shape> Process(IReadOnlyList<Shape> inputs)
            => new[] { inputs[0] };
    }
}
