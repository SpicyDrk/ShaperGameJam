using System;

namespace ShapeConnections.Simulation.Grid
{
    /// <summary>
    /// Integer cell address in a 2D grid. (0,0) is bottom-left.
    /// </summary>
    public readonly struct GridCoord : IEquatable<GridCoord>
    {
        public int Column { get; }
        public int Row { get; }

        public GridCoord(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public bool Equals(GridCoord other) => Column == other.Column && Row == other.Row;
        public override bool Equals(object obj) => obj is GridCoord other && Equals(other);
        public override int GetHashCode() => (Column * 397) ^ Row;
        public static bool operator ==(GridCoord left, GridCoord right) => left.Equals(right);
        public static bool operator !=(GridCoord left, GridCoord right) => !left.Equals(right);
        public override string ToString() => $"({Column}, {Row})";
    }
}
