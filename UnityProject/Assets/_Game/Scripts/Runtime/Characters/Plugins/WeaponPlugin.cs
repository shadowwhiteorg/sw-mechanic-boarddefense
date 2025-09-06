using _Game.Core.Events;
using UnityEngine;
using _Game.Enums;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Core;
using _Game.Runtime.Combat;

namespace _Game.Runtime.Characters.Plugins
{
    /// Weapon for DEFENSES. Finds targets per direction/range and fires on cooldown.
    /// Emits AttackPerformedEvent. If projectileMode=false, applies damage immediately (hitscan).
    public sealed class WeaponPlugin : IAttack
    {
        private readonly IEventBus _bus;
        private readonly CharacterRepository _repo;
        private readonly BoardGrid _grid;

        private readonly AttackDirection _direction;
        private readonly int   _rangeBlocks;
        private readonly float _fireRate;    // shots/sec
        private readonly int   _damage;

        private readonly bool  _projectileMode;
        private readonly float _projectileSpeed;
        private readonly int   _pierceCount;
        private readonly float _splashRadius;
        private readonly Vector3 _muzzleOffset;

        private CharacterEntity _self;
        private float _cooldown;

        // SO-driven
        public WeaponPlugin(IEventBus bus, CharacterRepository repo, BoardGrid grid,
                            AttackDirection direction, int rangeBlocks, WeaponConfig cfg)
        : this(bus, repo, grid, direction, rangeBlocks,
               cfg.fireRate, cfg.damage, cfg.projectileMode,
               cfg.projectileSpeed, cfg.pierceCount, cfg.splashRadius, cfg.muzzleOffset) {}

        // Direct values
        public WeaponPlugin(IEventBus bus, CharacterRepository repo, BoardGrid grid,
                            AttackDirection direction, int rangeBlocks,
                            float fireRate, int damage,
                            bool projectileMode = false, float projectileSpeed = 8f,
                            int pierceCount = 0, float splashRadius = 0f, Vector3 muzzleOffset = default)
        {
            _bus  = bus; _repo = repo; _grid = grid;
            _direction = direction;
            _rangeBlocks = Mathf.Max(0, rangeBlocks);
            _fireRate = Mathf.Max(0.05f, fireRate);
            _damage = Mathf.Max(1, damage);
            _projectileMode = projectileMode;
            _projectileSpeed = projectileSpeed;
            _pierceCount = Mathf.Max(0, pierceCount);
            _splashRadius = Mathf.Max(0f, splashRadius);
            _muzzleOffset = muzzleOffset;
        }

        public void OnSpawn(CharacterEntity e) { _self = e; _cooldown = 0f; }
        public void OnDespawn() { _self = null; }

        public void Tick(float dt)
        {
            if (_self == null) return;

            _cooldown -= dt;
            if (_cooldown > 0f) return;

            var target = (_direction == AttackDirection.Forward) ? AcquireForward() : AcquireOmni();
            if (target == null) { _cooldown = 0.05f; return; }

            // Muzzle world point
            var root = _self.View.Root;
            var muzzle = root.TransformPoint(_muzzleOffset);
            var targetWorld = target.View.Root.position;

            _bus.Fire(new AttackPerformedEvent(
                _self.EntityId, target.EntityId, muzzle, targetWorld,
                _damage, _projectileMode, _projectileSpeed, _pierceCount, _splashRadius));

            if (!_projectileMode)
            {
                // hitscan: apply immediately
                for (int i = 0; i < target.Plugins.Count; i++)
                    if (target.Plugins[i] is IHealth hp) { hp.ApplyDamage(_damage); break; }
            }

            _cooldown = 1f / _fireRate;
        }

        // Forward = first enemy above us in same column within range
        private CharacterEntity AcquireForward()
        {
            var s = _self.Cell;
            int maxRow = Mathf.Min(_grid.Size.Rows - 1, s.Row + _rangeBlocks);
            for (int r = s.Row + 1; r <= maxRow; r++)
            {
                var cell = new Cell(r, s.Col);
                if (_repo.TryGetByCell(cell, out var e) && e.Role == CharacterRole.Enemy) return e;
            }
            return null;
        }

        // All = nearest enemy by Manhattan distance within range
        private CharacterEntity AcquireOmni()
        {
            var s = _self.Cell;
            int rMin = Mathf.Max(0, s.Row - _rangeBlocks);
            int rMax = Mathf.Min(_grid.Size.Rows - 1, s.Row + _rangeBlocks);
            int cMin = Mathf.Max(0, s.Col - _rangeBlocks);
            int cMax = Mathf.Min(_grid.Size.Cols - 1, s.Col + _rangeBlocks);

            int best = int.MaxValue;
            CharacterEntity bestE = null;

            for (int r = rMin; r <= rMax; r++)
            for (int c = cMin; c <= cMax; c++)
            {
                var cell = new Cell(r, c);
                if (!_repo.TryGetByCell(cell, out var e) || e.Role != CharacterRole.Enemy) continue;
                int d = Mathf.Abs(r - s.Row) + Mathf.Abs(c - s.Col);
                if (d <= _rangeBlocks && d < best) { best = d; bestE = e; }
            }
            return bestE;
        }
    }
}
