using NUnit.Framework;
using ShapeConnections.Data;
using ShapeConnections.Simulation;

namespace ShapeConnections.DataTests
{
    public class ShapeDefinitionTests
    {
        [Test]
        public void ToShape_ReflectsAuthoredKindAndColor()
        {
            var def = ShapeDefinition.CreateForTests(ShapeKind.Triangle, ShapeColor.Blue);

            var shape = def.ToShape();

            Assert.That(shape.Kind, Is.EqualTo(ShapeKind.Triangle));
            Assert.That(shape.Color, Is.EqualTo(ShapeColor.Blue));
        }

        [Test]
        public void ToShape_DefaultDefinition_ReturnsSquareNone()
        {
            var def = UnityEngine.ScriptableObject.CreateInstance<ShapeDefinition>();

            var shape = def.ToShape();

            Assert.That(shape.Kind, Is.EqualTo(ShapeKind.Square));
            Assert.That(shape.Color, Is.EqualTo(ShapeColor.None));
        }
    }
}
