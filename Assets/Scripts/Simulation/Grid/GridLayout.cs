using System;

namespace ShapeConnections.Simulation.Grid
{
    /// <summary>
    /// Pure C# grid math: maps <see cref="GridCoord"/> ↔ world position for a
    /// grid of size (Width × Height) with the given <see cref="CellSize"/>.
    /// The grid is centered around the origin so the camera doesn't need to
    /// know about per-level sizes.
    /// </summary>
    public sealed class GridLayout
    {
        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }

        public GridLayout(int width, int height, float cellSize)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (cellSize <= 0f) throw new ArgumentOutOfRangeException(nameof(cellSize));

            Width = width;
            Height = height;
            CellSize = cellSize;
        }

        /// <summary>
        /// World position (x, y) of the centre of the given cell.
        /// Cell (0,0) is at the bottom-left of the grid in world space.
        /// </summary>
        public (float x, float y) WorldPosition(GridCoord coord)
        {
            var x = (coord.Column - (Width  - 1) * 0.5f) * CellSize;
            var y = (coord.Row    - (Height - 1) * 0.5f) * CellSize;
            return (x, y);
        }

        public bool InBounds(GridCoord coord)
            => coord.Column >= 0 && coord.Column < Width
            && coord.Row    >= 0 && coord.Row    < Height;
    }
}
