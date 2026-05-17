using System;

namespace ShapeConnections.Simulation
{
    /// <summary>
    /// Directed edge in the graph: a specific output slot of one node feeding a
    /// specific input slot of another. Source and target node ids may also refer
    /// to the synthetic level source/sink (see <see cref="Graph.SourceNodeId"/>
    /// and <see cref="Graph.SinkNodeId"/>).
    /// </summary>
    public readonly struct Connection : IEquatable<Connection>
    {
        public string FromNodeId { get; }
        public int FromOutputIndex { get; }
        public string ToNodeId { get; }
        public int ToInputIndex { get; }

        public Connection(string fromNodeId, int fromOutputIndex, string toNodeId, int toInputIndex)
        {
            if (string.IsNullOrEmpty(fromNodeId))
            {
                throw new ArgumentException("fromNodeId must be non-empty.", nameof(fromNodeId));
            }

            if (string.IsNullOrEmpty(toNodeId))
            {
                throw new ArgumentException("toNodeId must be non-empty.", nameof(toNodeId));
            }

            if (fromOutputIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fromOutputIndex));
            }

            if (toInputIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(toInputIndex));
            }

            FromNodeId = fromNodeId;
            FromOutputIndex = fromOutputIndex;
            ToNodeId = toNodeId;
            ToInputIndex = toInputIndex;
        }

        public bool Equals(Connection other)
            => FromNodeId == other.FromNodeId
               && FromOutputIndex == other.FromOutputIndex
               && ToNodeId == other.ToNodeId
               && ToInputIndex == other.ToInputIndex;

        public override bool Equals(object obj) => obj is Connection other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = FromNodeId?.GetHashCode() ?? 0;
                hash = (hash * 397) ^ FromOutputIndex;
                hash = (hash * 397) ^ (ToNodeId?.GetHashCode() ?? 0);
                hash = (hash * 397) ^ ToInputIndex;
                return hash;
            }
        }

        public static bool operator ==(Connection left, Connection right) => left.Equals(right);
        public static bool operator !=(Connection left, Connection right) => !left.Equals(right);

        public override string ToString() => $"{FromNodeId}[{FromOutputIndex}] → {ToNodeId}[{ToInputIndex}]";
    }
}
