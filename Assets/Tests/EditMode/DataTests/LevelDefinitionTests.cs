using System.Collections.Generic;
using NUnit.Framework;
using ShapeConnections.Data;
using ShapeConnections.Simulation;
using UnityEngine;

namespace ShapeConnections.DataTests
{
    public class LevelDefinitionTests
    {
        [Test]
        public void DefaultLevel_HasGrid5x5_AndEmptyLists()
        {
            var level = ScriptableObject.CreateInstance<LevelDefinition>();

            Assert.That(level.GridWidth, Is.EqualTo(5));
            Assert.That(level.GridHeight, Is.EqualTo(5));
            Assert.That(level.Inputs, Is.Empty);
            Assert.That(level.Targets, Is.Empty);
            Assert.That(level.Palette, Is.Empty);
        }

        [Test]
        public void CreateForTests_AssignsAllFields()
        {
            var input = ShapeDefinition.CreateForTests(ShapeKind.Square, ShapeColor.None);
            var target = ShapeDefinition.CreateForTests(ShapeKind.Square, ShapeColor.None);
            var nodeDef = ScriptableObject.CreateInstance<PassThroughNodeDefinition>();
            var palette = new List<LevelDefinition.PaletteEntry>
            {
                new LevelDefinition.PaletteEntry(nodeDef, 2)
            };

            var level = LevelDefinition.CreateForTests(
                width: 4, height: 6,
                inputs: new List<ShapeDefinition> { input },
                targets: new List<ShapeDefinition> { target },
                palette: palette);

            Assert.That(level.GridWidth, Is.EqualTo(4));
            Assert.That(level.GridHeight, Is.EqualTo(6));
            Assert.That(level.Inputs.Count, Is.EqualTo(1));
            Assert.That(level.Targets.Count, Is.EqualTo(1));
            Assert.That(level.Palette.Count, Is.EqualTo(1));
            Assert.That(level.Palette[0].Node, Is.SameAs(nodeDef));
            Assert.That(level.Palette[0].Count, Is.EqualTo(2));
        }
    }
}
