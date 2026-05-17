using System;

namespace ShapeConnections.Simulation
{
    /// <summary>
    /// A shape value travelling through the connection graph. Carries a
    /// <see cref="Kind"/> and an optional <see cref="Color"/>. Value-equal on both.
    /// </summary>
    public readonly struct Shape : IEquatable<Shape>
    {
        public ShapeKind Kind { get; }
        public ShapeColor Color { get; }

        public Shape(ShapeKind kind, ShapeColor color = ShapeColor.None)
        {
            Kind = kind;
            Color = color;
        }

        public bool Equals(Shape other) => Kind == other.Kind && Color == other.Color;

        public override bool Equals(object obj) => obj is Shape other && Equals(other);

        public override int GetHashCode() => ((int)Kind * 397) ^ (int)Color;

        public static bool operator ==(Shape left, Shape right) => left.Equals(right);

        public static bool operator !=(Shape left, Shape right) => !left.Equals(right);

        public override string ToString() => Color == ShapeColor.None ? Kind.ToString() : $"{Color} {Kind}";
    }
}
