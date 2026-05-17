using System.Collections.Generic;

namespace ShapeConnections.Simulation
{
    /// <summary>
    /// A processing node in the connection graph. Concrete subclasses define how
    /// inputs map to outputs in <see cref="Process"/>. Pure C# — no Unity dependency.
    /// </summary>
    public abstract class Node
    {
        public string Id { get; }
        public int InputSlotCount { get; }
        public int OutputSlotCount { get; }

        protected Node(string id, int inputSlotCount, int outputSlotCount)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new System.ArgumentException("Node id must be non-empty.", nameof(id));
            }

            if (inputSlotCount < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(inputSlotCount));
            }

            if (outputSlotCount < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(outputSlotCount));
            }

            Id = id;
            InputSlotCount = inputSlotCount;
            OutputSlotCount = outputSlotCount;
        }

        /// <summary>
        /// Compute outputs from the provided inputs. The returned list must have
        /// exactly <see cref="OutputSlotCount"/> entries.
        /// </summary>
        public abstract IReadOnlyList<Shape> Process(IReadOnlyList<Shape> inputs);
    }
}
