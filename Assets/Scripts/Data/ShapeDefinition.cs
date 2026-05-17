using ShapeConnections.Simulation;
using UnityEngine;

namespace ShapeConnections.Data
{
    /// <summary>
    /// Authored shape value: a (kind, color) pair that becomes a simulation
    /// <see cref="Shape"/>. Used by <see cref="LevelDefinition"/> for inputs
    /// and targets.
    /// </summary>
    [CreateAssetMenu(menuName = "Shape Connections/Shape Definition", fileName = "Shape")]
    public sealed class ShapeDefinition : ScriptableObject
    {
        [SerializeField] private ShapeKind kind = ShapeKind.Square;
        [SerializeField] private ShapeColor color = ShapeColor.None;

        public ShapeKind Kind => kind;
        public ShapeColor Color => color;

        public Shape ToShape() => new Shape(kind, color);

        /// <summary>Test-only helper. Lets tests build a definition without hitting the AssetDatabase.</summary>
        public static ShapeDefinition CreateForTests(ShapeKind kind, ShapeColor color)
        {
            var instance = CreateInstance<ShapeDefinition>();
            instance.kind = kind;
            instance.color = color;
            return instance;
        }
    }
}
