using NUnit.Framework;
using ShapeConnections.Data;
using ShapeConnections.Simulation;
using UnityEngine;

namespace ShapeConnections.DataTests
{
    public class NodeDefinitionTests
    {
        [Test]
        public void PassThroughNodeDefinition_CreatesPassThroughNodeWithGivenId()
        {
            var def = ScriptableObject.CreateInstance<PassThroughNodeDefinition>();

            var node = def.CreateNode("n1");

            Assert.That(node, Is.InstanceOf<PassThroughNode>());
            Assert.That(node.Id, Is.EqualTo("n1"));
            Assert.That(def.InputSlotCount, Is.EqualTo(1));
            Assert.That(def.OutputSlotCount, Is.EqualTo(1));
        }

        [Test]
        public void NodeDefinition_DisplayName_FallsBackToAssetName()
        {
            var def = ScriptableObject.CreateInstance<PassThroughNodeDefinition>();
            def.name = "Forward";

            Assert.That(def.DisplayName, Is.EqualTo("Forward"));
        }
    }
}
