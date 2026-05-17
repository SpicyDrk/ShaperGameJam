using NUnit.Framework;
using ShapeConnections.Simulation;

namespace ShapeConnections.Tests
{
    public class ShapeTests
    {
        [Test]
        public void Equality_SameKindAndColor_AreEqual()
        {
            var a = new Shape(ShapeKind.Square, ShapeColor.Red);
            var b = new Shape(ShapeKind.Square, ShapeColor.Red);

            Assert.That(a, Is.EqualTo(b));
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
            Assert.That(a == b, Is.True);
            Assert.That(a != b, Is.False);
        }

        [Test]
        public void Equality_DifferentKind_AreNotEqual()
        {
            var square = new Shape(ShapeKind.Square, ShapeColor.None);
            var triangle = new Shape(ShapeKind.Triangle, ShapeColor.None);

            Assert.That(square, Is.Not.EqualTo(triangle));
            Assert.That(square == triangle, Is.False);
            Assert.That(square != triangle, Is.True);
        }

        [Test]
        public void Equality_DifferentColor_AreNotEqual()
        {
            var red = new Shape(ShapeKind.Square, ShapeColor.Red);
            var blue = new Shape(ShapeKind.Square, ShapeColor.Blue);

            Assert.That(red, Is.Not.EqualTo(blue));
        }

        [Test]
        public void Default_HasSquareKindAndNoneColor()
        {
            var shape = default(Shape);

            Assert.That(shape.Kind, Is.EqualTo(ShapeKind.Square));
            Assert.That(shape.Color, Is.EqualTo(ShapeColor.None));
        }

        [Test]
        public void Constructor_WithKindOnly_DefaultsColorToNone()
        {
            var shape = new Shape(ShapeKind.Triangle);

            Assert.That(shape.Kind, Is.EqualTo(ShapeKind.Triangle));
            Assert.That(shape.Color, Is.EqualTo(ShapeColor.None));
        }

        [Test]
        public void ToString_IncludesKindAndColor()
        {
            var shape = new Shape(ShapeKind.Circle, ShapeColor.Yellow);

            var text = shape.ToString();

            Assert.That(text, Does.Contain("Circle"));
            Assert.That(text, Does.Contain("Yellow"));
        }
    }
}
