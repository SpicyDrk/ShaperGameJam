using ShapeConnections.Simulation;
using UnityEngine;

namespace ShapeConnections.Data
{
    /// <summary>
    /// Authored Cut node: maps one input shape to a (top, bottom) output pair.
    /// Each asset represents one specific cut (e.g., Square → Triangle/Triangle),
    /// mirroring the synth-modular "each module is one transformation" feel.
    /// </summary>
    [CreateAssetMenu(menuName = "Shape Connections/Nodes/Cut", fileName = "Cut")]
    public sealed class CutNodeDefinition : NodeDefinition
    {
        [SerializeField] private ShapeDefinition inputShape;
        [SerializeField] private ShapeDefinition topOutputShape;
        [SerializeField] private ShapeDefinition bottomOutputShape;

        public override int InputSlotCount => 1;
        public override int OutputSlotCount => 2;

        public ShapeDefinition InputShape => inputShape;
        public ShapeDefinition TopOutputShape => topOutputShape;
        public ShapeDefinition BottomOutputShape => bottomOutputShape;

        public override Node CreateNode(string id) => new CutNode(
            id,
            input:  inputShape  != null ? inputShape.ToShape()  : default,
            top:    topOutputShape    != null ? topOutputShape.ToShape()    : default,
            bottom: bottomOutputShape != null ? bottomOutputShape.ToShape() : default);

#if UNITY_EDITOR
        /// <summary>Editor-only direct-assignment hook used by SceneScaffolder.</summary>
        public void EditorConfigure(ShapeDefinition input, ShapeDefinition top, ShapeDefinition bottom)
        {
            inputShape = input;
            topOutputShape = top;
            bottomOutputShape = bottom;
        }
#endif
    }
}
