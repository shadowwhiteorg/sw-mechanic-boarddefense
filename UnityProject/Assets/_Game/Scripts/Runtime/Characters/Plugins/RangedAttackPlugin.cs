using UnityEngine;
using _Game.Enums;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Core;

namespace _Game.Runtime.Characters.Plugins
{
    /// <summary>
    /// Defenses fire straight UP their current column (toward higher row indices),
    /// hitting the FIRST enemy found within range. No projectiles required.
    /// Cooldown is driven by attackRate (attacks/sec); damage uses attackDamage (int).
    /// </summary>
    public sealed class RangedAttackPlugin : IAttack
    {
        private readonly CharacterRepository _repo;
        private readonly BoardGrid _grid;
        private readonly float _rate;        // attacks/sec
        private readonly int _damage;        // damage per shot
        private readonly int _maxRangeCells; // how many rows to scan upward

        private CharacterEntity _self;
        private float _cooldown;

        public RangedAttackPlugin(CharacterRepository repo,
                                  BoardGrid grid,
                                  float rate,
                                  int damage,
                                  int maxRangeCells = int.MaxValue)
        {
            _repo = repo;
            _grid = grid;
            _rate = Mathf.Max(0.05f, rate);
            _damage = Mathf.Max(1, damage);
            _maxRangeCells = maxRangeCells < 1 ? int.MaxValue : maxRangeCells;
        }

        public void OnSpawn(CharacterEntity e)
        {
            _self = e;
            _cooldown = 0f;
        }

        public void OnDespawn()
        {
            _self = null;
        }

        public void Tick(float dt)
        {
            if (_self == null) return;

            _cooldown -= dt;
            if (_cooldown > 0f) return;

            var target = AcquireTargetUpColumn();
            if (target != null)
            {
                DealDamage(target, _damage);
                _cooldown = 1f / _rate;
            }
            else
            {
                // No target; try again next frame (don’t reset cooldown fully).
                _cooldown = 0.05f;
            }
        }

        /// <summary>Find the nearest ENEMY in the same column, above us (row+).</summary>
        private CharacterEntity AcquireTargetUpColumn()
        {
            var c = _self.Cell; // our current grid cell
            // enemies spawn at the top row (larger row index) and march down toward row 0.
            int maxRow = _grid.Size.Rows - 1;
            int steps = 0;

            for (int r = c.Row + 1; r <= maxRow && steps < _maxRangeCells; r++, steps++)
            {
                var probe = new Cell(r, c.Col);
                if (_repo.TryGetByCell(probe, out var candidate) && candidate.Role == CharacterRole.Enemy)
                    return candidate;
            }
            return null;
        }

        private static void DealDamage(CharacterEntity target, int amount)
        {
            // Find an IHealth plugin on the target and apply damage.
            IHealth hp = null;
            var plugins = target.Plugins; // IReadOnlyList<ICharacterPlugin>
            for (int i = 0; i < plugins.Count; i++)
            {
                if (plugins[i] is IHealth h) { hp = h; break; }
            }
            hp?.ApplyDamage(amount);
            Debug.Log($"Ops I did it again! {target.EntityId}");
        }
    }
}
