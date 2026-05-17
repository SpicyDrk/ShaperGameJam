using System;
using System.Collections.Generic;

namespace ShapeConnections.Simulation
{
    /// <summary>
    /// Decides whether a player's computed outputs satisfy a level's target outputs.
    /// </summary>
    /// <remarks>
    /// Target <see cref="ShapeColor.None"/> is treated as a wildcard — any computed
    /// color satisfies it. This lets shape-only MVP levels and color-aware levels
    /// share the same comparator: an early level that doesn't care about color
    /// authors its targets with <see cref="ShapeColor.None"/>, while a Color-node
    /// level authors targets with a specific color.
    /// </remarks>
    public static class TargetComparator
    {
        public static bool Matches(IReadOnlyList<Shape> computed, IReadOnlyList<Shape> target)
        {
            if (computed == null) throw new ArgumentNullException(nameof(computed));
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (computed.Count != target.Count) return false;

            for (var i = 0; i < computed.Count; i++)
            {
                if (computed[i].Kind != target[i].Kind) return false;
                if (target[i].Color != ShapeColor.None && computed[i].Color != target[i].Color) return false;
            }

            return true;
        }
    }
}
