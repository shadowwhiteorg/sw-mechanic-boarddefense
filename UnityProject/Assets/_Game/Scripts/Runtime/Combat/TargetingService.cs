using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Core;

namespace _Game.Runtime.Combat
{
    public sealed class TargetingService
    {
        private readonly BoardGrid _grid;
        private readonly CharacterRepository _repo;

        public TargetingService(BoardGrid grid, CharacterRepository repo)
        {
            _grid = grid; _repo = repo;
        }

        public bool TryFindForwardEnemy(CharacterEntity self, int rangeBlocks, out CharacterEntity target)
        {
            target = null;
            var start = self.Cell;
            int maxRow = System.Math.Min(_grid.Size.Rows - 1, start.Row + rangeBlocks);

            for (int r = start.Row + 1; r <= maxRow; r++)
            {
                var c = new Cell(r, start.Col);
                if (_repo.TryGetByCell(c, out var e) && e.Role != self.Role)
                {
                    target = e;
                    return true;
                }
            }
            return false;
        }

        public bool TryFindOmniEnemy(CharacterEntity self, int rangeBlocks, out CharacterEntity target)
        {
            target = null;
            var s = self.Cell;
            int rMin = System.Math.Max(0, s.Row - rangeBlocks);
            int rMax = System.Math.Min(_grid.Size.Rows - 1, s.Row + rangeBlocks);
            int cMin = System.Math.Max(0, s.Col - rangeBlocks);
            int cMax = System.Math.Min(_grid.Size.Cols - 1, s.Col + rangeBlocks);

            int bestDist = int.MaxValue;
            CharacterEntity best = null;

            for (int r = rMin; r <= rMax; r++)
            for (int c = cMin; c <= cMax; c++)
            {
                var cell = new Cell(r, c);
                if (!_repo.TryGetByCell(cell, out var e) || e.Role == self.Role) continue;

                int dist = System.Math.Abs(r - s.Row) + System.Math.Abs(c - s.Col); // Manhattan
                if (dist <= rangeBlocks && dist < bestDist)
                {
                    bestDist = dist;
                    best = e;
                }
            }

            if (best != null) { target = best; return true; }
            return false;
        }
    }
}
