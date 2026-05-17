using ShapeConnections.Simulation;
using UnityEngine;

namespace ShapeConnections.Data
{
    /// <summary>
    /// Authored node template. Subclassed by <see cref="PassThroughNodeDefinition"/>
    /// (and later by Cut / Combine / Color variants) to bridge inspector-authored
    /// configuration to runtime <see cref="Node"/> instances.
    /// </summary>
    public abstract class NodeDefinition : ScriptableObject
    {
        [SerializeField] private string displayName;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;

        public abstract int InputSlotCount { get; }
        public abstract int OutputSlotCount { get; }

        /// <summary>Create a runtime <see cref="Node"/> with the given id.</summary>
        public abstract Node CreateNode(string id);
    }
}
