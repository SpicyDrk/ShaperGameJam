using NUnit.Framework;
using ShapeConnections.Simulation;

namespace ShapeConnections.Tests
{
    public class TargetComparatorTests
    {
        // SC-03: Target comparison detects pass and fail.

        [Test]
        public void Match_SameShapesNoColor_Passes()
        {
            var computed = new[] { new Shape(ShapeKind.Square), new Shape(ShapeKind.Triangle) };
            var target   = new[] { new Shape(ShapeKind.Square), new Shape(ShapeKind.Triangle) };

            Assert.That(TargetComparator.Matches(computed, target), Is.True);
        }

        [Test]
        public void Match_SameShapesAndColors_Passes()
        {
            var computed = new[] { new Shape(ShapeKind.Square, ShapeColor.Red) };
            var target   = new[] { new Shape(ShapeKind.Square, ShapeColor.Red) };

            Assert.That(TargetComparator.Matches(computed, target), Is.True);
        }

        [Test]
        public void Match_TargetColorNone_IsWildcard_AcceptsAnyComputedColor()
        {
            var computed = new[] { new Shape(ShapeKind.Square, ShapeColor.Red) };
            var target   = new[] { new Shape(ShapeKind.Square, ShapeColor.None) };

            Assert.That(TargetComparator.Matches(computed, target), Is.True);
        }

        [Test]
        public void Mismatch_DifferentKind_Fails()
        {
            var computed = new[] { new Shape(ShapeKind.Square) };
            var target   = new[] { new Shape(ShapeKind.Triangle) };

            Assert.That(TargetComparator.Matches(computed, target), Is.False);
        }

        [Test]
        public void Mismatch_DifferentColorWhenTargetColorIsSet_Fails()
        {
            var computed = new[] { new Shape(ShapeKind.Square, ShapeColor.Red) };
            var target   = new[] { new Shape(ShapeKind.Square, ShapeColor.Blue) };

            Assert.That(TargetComparator.Matches(computed, target), Is.False);
        }

        [Test]
        public void Mismatch_DifferentLength_Fails()
        {
            var computed = new[] { new Shape(ShapeKind.Square) };
            var target   = new[] { new Shape(ShapeKind.Square), new Shape(ShapeKind.Triangle) };

            Assert.That(TargetComparator.Matches(computed, target), Is.False);
        }

        [Test]
        public void Match_BothEmpty_Passes()
        {
            Assert.That(
                TargetComparator.Matches(System.Array.Empty<Shape>(), System.Array.Empty<Shape>()),
                Is.True);
        }
    }
}
