using UnityEngine;
using _Game.Enums;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Core;

namespace _Game.Runtime.Characters.Plugins
{
    public sealed class RangedAttackPlugin : IAttack
    {
        private readonly CharacterRepository _repo;
        private readonly BoardGrid _grid;
        private readonly AttackDirection _direction;
        private readonly int _maxRangeCells;
        private WeaponPlugin _weapon;

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

            int step = (_self.Role == CharacterRole.Enemy) ? -1 : +1;

            int rStart = s.Row + step;
            int rEndExclusive = (_self.Role == CharacterRole.Enemy)
                ? Mathf.Max(0, s.Row - _maxRangeCells) - 1  
                : Mathf.Min(_grid.Size.Rows - 1, s.Row + _maxRangeCells) + 1;

            for (int r = rStart; r != rEndExclusive; r += step)
            {
                if (r < 0 || r >= _grid.Size.Rows) break;

                var c = new Cell(r, s.Col);
                if (_repo.TryGetByCell(c, out var e) && e.Role != _self.Role)
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

            int bestManhattan = int.MaxValue;
            CharacterEntity best = null;

            for (int r = rMin; r <= rMax; r++)
            for (int c = cMin; c <= cMax; c++)
            {
                var cell = new Cell(r, c);
                if (!_repo.TryGetByCell(cell, out var e)) continue;
                if (e.Role == _self.Role) continue;

                int d = Mathf.Abs(r - s.Row) + Mathf.Abs(c - s.Col);
                if (d <= _maxRangeCells && d < bestManhattan)
                {
                    bestManhattan = d;
                    best = e;
                }
            }
            return best;
        }
    }
}
