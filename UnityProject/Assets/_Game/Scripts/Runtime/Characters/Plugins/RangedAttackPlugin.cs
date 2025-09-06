// Assets/_Game/Scripts/Runtime/Characters/Plugins/RangedAttackPlugin.cs
using UnityEngine;
using _Game.Enums;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;

namespace _Game.Runtime.Characters.Plugins
{
    /// Performs target acquisition (Forward or Omni) and orders WeaponPlugin to fire.
    public sealed class RangedAttackPlugin : IAttack
    {
        private readonly CharacterRepository _repo;
        private readonly BoardGrid _grid;
        private readonly AttackDirection _direction;
        private readonly int _maxRangeCells;
        private WeaponPlugin _weapon; // injected after both plugins are created

        private CharacterEntity _self;

        public RangedAttackPlugin(CharacterRepository repo, BoardGrid grid, AttackDirection direction, int maxRangeCells)
        {
            _repo = repo; _grid = grid; _direction = direction; 
            _maxRangeCells = Mathf.Max(0, maxRangeCells);
        }

        public void BindWeapon(WeaponPlugin weapon) => _weapon = weapon;

        public void OnSpawn(CharacterEntity e) { _self = e; }
        public void OnDespawn() { _self = null; _weapon = null; }

        public void Tick(float dt)
        {
            if (_self == null || _weapon == null || !_weapon.IsReady) return;

            var target = (_direction == AttackDirection.Forward) ? AcquireForward() : AcquireOmni();
            if (target != null) _weapon.TryFireAt(target);
        }

        private CharacterEntity AcquireForward()
        {
            var s = _self.Cell;
            int maxRow = Mathf.Min(_grid.Size.Rows - 1, s.Row + _maxRangeCells);
            for (int r = s.Row + 1; r <= maxRow; r++)
            {
                var c = new _Game.Runtime.Core.Cell(r, s.Col);
                if (_repo.TryGetByCell(c, out var e) && e.Role == _Game.Enums.CharacterRole.Enemy)
                    return e;
            }
            return null;
        }

        private CharacterEntity AcquireOmni()
        {
            var s = _self.Cell;
            int rMin = Mathf.Max(0, s.Row - _maxRangeCells);
            int rMax = Mathf.Min(_grid.Size.Rows - 1, s.Row + _maxRangeCells);
            int cMin = Mathf.Max(0, s.Col - _maxRangeCells);
            int cMax = Mathf.Min(_grid.Size.Cols - 1, s.Col + _maxRangeCells);

            int best = int.MaxValue; CharacterEntity bestE = null;
            for (int r = rMin; r <= rMax; r++)
            for (int c = cMin; c <= cMax; c++)
            {
                var cell = new _Game.Runtime.Core.Cell(r, c);
                if (!_repo.TryGetByCell(cell, out var e) || e.Role != _Game.Enums.CharacterRole.Enemy) continue;
                int d = Mathf.Abs(r - s.Row) + Mathf.Abs(c - s.Col);
                if (d <= _maxRangeCells && d < best) { best = d; bestE = e; }
            }
            return bestE;
        }
    }
}
