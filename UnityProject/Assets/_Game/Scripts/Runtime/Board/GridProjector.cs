using UnityEngine;
using _Game.Runtime.Core;

namespace _Game.Runtime.Board
{
    public sealed class GridProjector
    {
        private readonly BoardGrid _grid;
        private readonly BoardSurface _surface;

        public GridProjector(BoardGrid grid, BoardSurface surface)
        {
            _grid = grid; _surface = surface;
        }

        public bool TryWorldToCell(Vector3 world, out Cell cell)
        {
            var local = _surface.WorldToLocal(world) - _surface.localOrigin;
            return _grid.TryLocalToCell(local, out cell);
        }

        public Vector3 CellToWorldCenter(Cell cell)
        {
            var localCenter = _grid.CellToLocalCenter(cell) + _surface.localOrigin;
            return _surface.LocalToWorld(localCenter);
        }
    }
}