using System.Collections.Generic;
using NUnit.Framework;
using ShapeConnections.Data;
using ShapeConnections.Simulation;

namespace ShapeConnections.DataTests
{
    public class CutNodeDefinitionTests
    {
        [Test]
        public void Definition_HasOneInputAndTwoOutputs()
        {
            var def = UnityEngine.ScriptableObject.CreateInstance<CutNodeDefinition>();

            Assert.That(def.InputSlotCount, Is.EqualTo(1));
            Assert.That(def.OutputSlotCount, Is.EqualTo(2));
        }

        [Test]
        public void CreateNode_BuildsCutNodeWithConfiguredShapes()
        {
            var def = UnityEngine.ScriptableObject.CreateInstance<CutNodeDefinition>();
            var square   = ShapeDefinition.CreateForTests(ShapeKind.Square,   ShapeColor.None);
            var triangle = ShapeDefinition.CreateForTests(ShapeKind.Triangle, ShapeColor.None);
            def.EditorConfigure(input: square, top: triangle, bottom: triangle);

            var node = def.CreateNode("c1");

            Assert.That(node, Is.InstanceOf<CutNode>());
            Assert.That(node.Id, Is.EqualTo("c1"));

            // Round-trip through Process: authored Square input → (Triangle, Triangle).
            var outputs = node.Process(new List<Shape> { new Shape(ShapeKind.Square) });
            Assert.That(outputs[0], Is.EqualTo(new Shape(ShapeKind.Triangle)));
            Assert.That(outputs[1], Is.EqualTo(new Shape(ShapeKind.Triangle)));
        }

        [Test]
        public void CreateNode_PreservesAuthoredOrder_TopVsBottom()
        {
            var def = UnityEngine.ScriptableObject.CreateInstance<CutNodeDefinition>();
            var square   = ShapeDefinition.CreateForTests(ShapeKind.Square,   ShapeColor.None);
            var triangle = ShapeDefinition.CreateForTests(ShapeKind.Triangle, ShapeColor.None);
            var circle   = ShapeDefinition.CreateForTests(ShapeKind.Circle,   ShapeColor.None);
            def.EditorConfigure(input: square, top: triangle, bottom: circle);

            var node = def.CreateNode("c1");
            var outputs = node.Process(new List<Shape> { new Shape(ShapeKind.Square) });

            Assert.That(outputs[0], Is.EqualTo(new Shape(ShapeKind.Triangle)), "top output came from topOutputShape");
            Assert.That(outputs[1], Is.EqualTo(new Shape(ShapeKind.Circle)),   "bottom output came from bottomOutputShape");
        }
    }
}
