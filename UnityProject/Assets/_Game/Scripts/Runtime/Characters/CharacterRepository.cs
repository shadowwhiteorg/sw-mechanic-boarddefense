using System.Collections.Generic;
using _Game.Runtime.Core;

namespace _Game.Runtime.Characters
{
    public sealed class CharacterRepository
    {
        private readonly Dictionary<int, CharacterEntity> _byId = new();
        private readonly Dictionary<Cell, int> _byCell = new();

        public bool TryGetByCell(Cell cell, out CharacterEntity e)
        {
            e = null;
            return _byCell.TryGetValue(cell, out var id) && _byId.TryGetValue(id, out e);
        }
        
        public bool TryGetById(int id, out CharacterEntity e)
        {
            return _byId.TryGetValue(id, out e);
        }

        public bool IsOccupied(Cell c) => _byCell.ContainsKey(c);

        public void Add(CharacterEntity e, Cell c)
        {
            _byId[e.EntityId] = e;
            _byCell[c] = e.EntityId;
        }

        public void Remove(CharacterEntity e)
        {
            _byId.Remove(e.EntityId);
            if (_byCell.TryGetValue(e.Cell, out var id) && id == e.EntityId)
                _byCell.Remove(e.Cell);
        }
    }
}