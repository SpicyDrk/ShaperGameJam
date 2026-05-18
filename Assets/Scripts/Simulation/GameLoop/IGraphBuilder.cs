using System.Collections.Generic;

namespace ShapeConnections.Simulation.GameLoop
{
    /// <summary>
    /// Source of the connection graph to evaluate. Defaults to building from
    /// the current scene (NodeViews + WireViews); alternates could build from
    /// a replay file, a test fixture, or a procedural generator.
    /// </summary>
    public interface IGraphBuilder
    {
        /// <summary>Build a fresh <see cref="Graph"/> reflecting current state.</summary>
        Graph Build();

        /// <summary>Input shapes at the level's input sockets, indexed by socket index.</summary>
        IReadOnlyList<Shape> InputShapes { get; }

        /// <summary>How many output sockets the level expects to produce.</summary>
        int OutputSocketCount { get; }
    }
}
