using ShapeConnections.Simulation.Grid;
using UnityEngine;
using GridLayout = ShapeConnections.Simulation.Grid.GridLayout;

namespace ShapeConnections.Game.Grid
{
    /// <summary>
    /// Spawns visual cell tiles for a <see cref="GridLayout"/>. Thin Unity adapter —
    /// all coordinate math lives in the pure <see cref="GridLayout"/>, so the same
    /// layout can be used by tests, by alternate renderers, or by future runtime
    /// solvers without instantiating any MonoBehaviour.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GridView : MonoBehaviour
    {
        [SerializeField, Tooltip("Optional sprite for each grid cell. Defaults to a built-in white square if null.")]
        private Sprite cellSprite;

        [SerializeField, Tooltip("Color tint applied to every cell.")]
        private Color cellColor = new Color(1f, 1f, 1f, 0.12f);

        private GridLayout _layout;
        private Transform _cellsRoot;

        public GridLayout Layout => _layout;

        public void Bind(int width, int height, float cellSize)
        {
            _layout = new GridLayout(width, height, cellSize);
            Rebuild();
        }

        public Vector3 WorldPositionOf(GridCoord coord)
        {
            if (_layout == null)
            {
                throw new System.InvalidOperationException("GridView.Bind must be called before WorldPositionOf.");
            }

            var (x, y) = _layout.WorldPosition(coord);
            return transform.position + new Vector3(x, y, 0f);
        }

        private void Rebuild()
        {
            if (_cellsRoot != null)
            {
                if (Application.isPlaying) Destroy(_cellsRoot.gameObject);
                else DestroyImmediate(_cellsRoot.gameObject);
            }

            var root = new GameObject("Cells");
            root.transform.SetParent(transform, worldPositionStays: false);
            _cellsRoot = root.transform;

            for (var row = 0; row < _layout.Height; row++)
            {
                for (var col = 0; col < _layout.Width; col++)
                {
                    var coord = new GridCoord(col, row);
                    var cell = new GameObject($"Cell ({col},{row})");
                    cell.transform.SetParent(_cellsRoot, worldPositionStays: false);
                    cell.transform.localPosition = WorldPositionOf(coord) - transform.position;

                    var sr = cell.AddComponent<SpriteRenderer>();
                    sr.sprite = cellSprite != null ? cellSprite : BuiltinSquareSprite();
                    sr.color = cellColor;
                    sr.drawMode = SpriteDrawMode.Sliced;
                    sr.size = new Vector2(_layout.CellSize * 0.95f, _layout.CellSize * 0.95f);
                    sr.sortingOrder = 0; // explicit: cells are the bottom of the visual stack
                }
            }
        }

        private static Sprite _fallback;
        private static Sprite BuiltinSquareSprite()
        {
            if (_fallback != null) return _fallback;
            var tex = Texture2D.whiteTexture;
            _fallback = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            return _fallback;
        }
    }
}
