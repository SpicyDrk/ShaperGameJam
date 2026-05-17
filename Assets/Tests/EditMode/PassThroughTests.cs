using System.Collections.Generic;
using NUnit.Framework;
using ShapeConnections.Simulation;

namespace ShapeConnections.Tests
{
    public class PassThroughTests
    {
        [Test]
        public void Process_ForwardsInputUnchanged()
        {
            var node = new PassThroughNode("pt");
            var input = new Shape(ShapeKind.Square, ShapeColor.Red);

            var outputs = node.Process(new List<Shape> { input });

            Assert.That(outputs.Count, Is.EqualTo(1));
            Assert.That(outputs[0], Is.EqualTo(input));
        }

        [Test]
        public void Construction_HasOneInputAndOneOutput()
        {
            var node = new PassThroughNode("pt");

            Assert.That(node.InputSlotCount, Is.EqualTo(1));
            Assert.That(node.OutputSlotCount, Is.EqualTo(1));
        }
    }
}
