using NUnit.Framework;
using ShapeConnections.Simulation;
using ShapeConnections.Simulation.Grid;

namespace ShapeConnections.Tests
{
    public class GridLayoutTests
    {
        [Test]
        public void Construction_StoresDimensions()
        {
            var layout = new GridLayout(width: 5, height: 4, cellSize: 1.5f);

            Assert.That(layout.Width, Is.EqualTo(5));
            Assert.That(layout.Height, Is.EqualTo(4));
            Assert.That(layout.CellSize, Is.EqualTo(1.5f));
        }

        [Test]
        public void Construction_RejectsNonPositive()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new GridLayout(0, 3, 1f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new GridLayout(3, 0, 1f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new GridLayout(3, 3, 0f));
        }

        [Test]
        public void WorldPosition_OddGrid_CenterCellIsOrigin()
        {
            // 3x3, cellSize=1: center cell is (1,1), expected world (0,0)
            var layout = new GridLayout(3, 3, 1f);

            var (x, y) = layout.WorldPosition(new GridCoord(1, 1));

            Assert.That(x, Is.EqualTo(0f).Within(1e-5f));
            Assert.That(y, Is.EqualTo(0f).Within(1e-5f));
        }

        [Test]
        public void WorldPosition_EvenGrid_CentersBetweenCells()
        {
            // 4x4 cellSize=1: cells span columns 0..3, centered around column 1.5
            // Cell (0,0) sits at (-1.5, -1.5). Cell (3,3) sits at (1.5, 1.5).
            var layout = new GridLayout(4, 4, 1f);

            var (xMin, yMin) = layout.WorldPosition(new GridCoord(0, 0));
            var (xMax, yMax) = layout.WorldPosition(new GridCoord(3, 3));

            Assert.That(xMin, Is.EqualTo(-1.5f).Within(1e-5f));
            Assert.That(yMin, Is.EqualTo(-1.5f).Within(1e-5f));
            Assert.That(xMax, Is.EqualTo(1.5f).Within(1e-5f));
            Assert.That(yMax, Is.EqualTo(1.5f).Within(1e-5f));
        }

        [Test]
        public void WorldPosition_RespectsCellSize()
        {
            var layout = new GridLayout(3, 3, 2f);

            var (x, _) = layout.WorldPosition(new GridCoord(2, 1));

            Assert.That(x, Is.EqualTo(2f).Within(1e-5f));
        }

        [Test]
        public void InBounds_ReturnsTrueForValidCoords()
        {
            var layout = new GridLayout(3, 3, 1f);

            Assert.That(layout.InBounds(new GridCoord(0, 0)), Is.True);
            Assert.That(layout.InBounds(new GridCoord(2, 2)), Is.True);
            Assert.That(layout.InBounds(new GridCoord(1, 1)), Is.True);
        }

        [Test]
        public void InBounds_ReturnsFalseForOutOfRangeCoords()
        {
            var layout = new GridLayout(3, 3, 1f);

            Assert.That(layout.InBounds(new GridCoord(-1, 0)), Is.False);
            Assert.That(layout.InBounds(new GridCoord(3, 0)), Is.False);
            Assert.That(layout.InBounds(new GridCoord(0, -1)), Is.False);
            Assert.That(layout.InBounds(new GridCoord(0, 3)), Is.False);
        }
    }

    public class GridCoordTests
    {
        [Test]
        public void Equality_SameColumnAndRow_AreEqual()
        {
            var a = new GridCoord(2, 3);
            var b = new GridCoord(2, 3);

            Assert.That(a, Is.EqualTo(b));
            Assert.That(a == b, Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equality_DifferentValues_AreNotEqual()
        {
            Assert.That(new GridCoord(1, 2), Is.Not.EqualTo(new GridCoord(2, 1)));
        }

        [Test]
        public void ToString_IsReadable()
        {
            var coord = new GridCoord(3, 4);

            Assert.That(coord.ToString(), Does.Contain("3").And.Contain("4"));
        }
    }
}
