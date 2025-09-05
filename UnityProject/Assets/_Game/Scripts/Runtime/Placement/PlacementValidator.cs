using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Core;
using UnityEngine;

namespace _Game.Runtime.Placement
{
    public class PlacementValidator
    {
        private readonly CharacterRepository _repo;
        private readonly BoardGrid _grid;

        public PlacementValidator(CharacterRepository repo, BoardGrid grid)
        {
            _repo = repo;
            _grid = grid;
        }

        public bool IsValid(Cell cell)
        {
            return _grid.InBounds(cell) && !_repo.IsOccupied(cell);
        }
    }
}