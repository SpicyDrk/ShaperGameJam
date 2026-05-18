using System.Collections.Generic;

namespace ShapeConnections.Simulation
{
    /// <summary>
    /// Splits a single authored input shape into two authored output shapes
    /// (top, bottom) in fixed slot order. If the runtime input doesn't match
    /// the authored input, both outputs are <c>default(Shape)</c> — the
    /// documented fallback that mirrors Combine (Phase 4 SC-08).
    /// </summary>
    public sealed class CutNode : Node
    {
        private readonly Shape _input;
        private readonly Shape _top;
        private readonly Shape _bottom;

        public CutNode(string id, Shape input, Shape top, Shape bottom)
            : base(id, inputSlotCount: 1, outputSlotCount: 2)
        {
            _input = input;
            _top = top;
            _bottom = bottom;
        }

        public override IReadOnlyList<Shape> Process(IReadOnlyList<Shape> inputs)
        {
            if (inputs.Count > 0 && inputs[0].Equals(_input))
            {
                return new[] { _top, _bottom };
            }
            return new[] { default(Shape), default(Shape) };
        }
    }
}
