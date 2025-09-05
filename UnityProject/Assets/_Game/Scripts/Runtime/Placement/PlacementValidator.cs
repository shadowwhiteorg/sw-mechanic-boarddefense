using _Game.Runtime.Board;
using _Game.Runtime.Core;
using _Game.Runtime.Characters;

namespace _Game.Runtime.Placement
{
    public sealed class PlacementValidator
    {
        private readonly CharacterRepository _repo;
        private readonly BoardGrid _grid;

        public PlacementValidator(CharacterRepository repo, BoardGrid grid)
        {
            _repo = repo; _grid = grid;
        }

        public bool IsValid(Cell cell)
        {
            if (!_grid.InBounds(cell)) return false;
            if (!_grid.IsDefensePlacementAllowed(cell)) return false;
            if (_repo.IsOccupied(cell)) return false;
            return true;
        }
    }
}