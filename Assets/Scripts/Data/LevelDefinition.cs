using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShapeConnections.Data
{
    /// <summary>
    /// Per-level authored data: grid size, edge input shapes, target outputs, and
    /// the palette of nodes the player may place. Skeleton only — Phase 1 ships
    /// the data shape; later phases wire it to the runtime <see cref="Simulation.Graph"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Shape Connections/Level Definition", fileName = "Level")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField, Min(3)] private int gridWidth = 5;
        [SerializeField, Min(3)] private int gridHeight = 5;

        [Tooltip("Shapes that appear at the level's input sockets (left edge).")]
        [SerializeField] private List<ShapeDefinition> inputs = new List<ShapeDefinition>();

        [Tooltip("Shapes that must be produced at the level's output sockets (right edge).")]
        [SerializeField] private List<ShapeDefinition> targets = new List<ShapeDefinition>();

        [Tooltip("Node types and counts the player may place on the grid.")]
        [SerializeField] private List<PaletteEntry> palette = new List<PaletteEntry>();

        [TextArea(2, 5), SerializeField] private string designerNotes;

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public IReadOnlyList<ShapeDefinition> Inputs => inputs;
        public IReadOnlyList<ShapeDefinition> Targets => targets;
        public IReadOnlyList<PaletteEntry> Palette => palette;
        public string DesignerNotes => designerNotes;

        [Serializable]
        public sealed class PaletteEntry
        {
            [SerializeField] private NodeDefinition node;
            [SerializeField, Min(0)] private int count = 1;

            public NodeDefinition Node => node;
            public int Count => count;

            public PaletteEntry() { }

            public PaletteEntry(NodeDefinition node, int count)
            {
                this.node = node;
                this.count = count;
            }
        }

        /// <summary>Test-only helper. Lets tests build a level without hitting the AssetDatabase.</summary>
        public static LevelDefinition CreateForTests(
            int width, int height,
            List<ShapeDefinition> inputs,
            List<ShapeDefinition> targets,
            List<PaletteEntry> palette)
        {
            var instance = CreateInstance<LevelDefinition>();
            instance.gridWidth = width;
            instance.gridHeight = height;
            instance.inputs = inputs ?? new List<ShapeDefinition>();
            instance.targets = targets ?? new List<ShapeDefinition>();
            instance.palette = palette ?? new List<PaletteEntry>();
            return instance;
        }
    }
}
