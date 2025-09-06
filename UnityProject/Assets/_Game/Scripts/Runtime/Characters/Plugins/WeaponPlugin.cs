// Assets/_Game/Scripts/Runtime/Characters/Plugins/WeaponPlugin.cs
using _Game.Core.Events;
using UnityEngine;
using _Game.Enums;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Combat;

namespace _Game.Runtime.Characters.Plugins
{
    /// Weapon now focuses on cadence & firing when ordered.
    /// Targeting is delegated to RangedAttackPlugin.
    public sealed class WeaponPlugin : IAttack
    {
        private readonly IEventBus _bus;
        private readonly BoardGrid _grid;

        private readonly AttackDirection _direction;
        private readonly int   _rangeBlocks;
        private readonly WeaponConfig _cfg;

        private CharacterEntity _self;
        private float _cooldown;

        public WeaponPlugin(IEventBus bus, BoardGrid grid, AttackDirection direction, int rangeBlocks, WeaponConfig cfg)
        {
            _bus = bus; _grid = grid; _direction = direction;
            _rangeBlocks = Mathf.Max(0, rangeBlocks);
            _cfg = cfg;
        }

        public void OnSpawn(CharacterEntity e) { _self = e; _cooldown = 0f; }
        public void OnDespawn() { _self = null; }
        public void Tick(float dt) { _cooldown -= dt; }

        public bool IsReady => _cooldown <= 0f;
        public int  RangeBlocks => _rangeBlocks;
        public AttackDirection Direction => _direction;

        /// Order the weapon to fire at a specific target (already acquired by another plugin).
        public bool TryFireAt(CharacterEntity target)
        {
            Debug.Log("I break your heat");
            if (_self == null || target == null || !IsReady) return false;

            // Validate simple range (Manhattan) before firing.
            var s = _self.Cell; var t = target.Cell;
            int md = Mathf.Abs(s.Row - t.Row) + Mathf.Abs(s.Col - t.Col);
            if (md > _rangeBlocks) return false;

            // Use Projectile SO as single source of damage/flight.
            var proj = _cfg.projectileConfig;
            int damage       = proj ? proj.damage : 1;
            float speed      = proj ? proj.speed  : 8f;
            int pierce       = proj ? proj.pierceCount  : 0;
            float splash     = proj ? proj.splashRadius : 0f;

            // Build muzzle/aim
            var muzzle = _self.View.Root.TransformPoint(_cfg.muzzleOffset);
            var targetWorld = target.View.Root.position;

            _bus.Fire(new AttackPerformedEvent(
                _self.EntityId, target.EntityId, muzzle, targetWorld,
                damage, _cfg.projectileMode, speed, pierce, splash, proj));

            // Hitscan path: apply damage immediately using projectile.damage as the source of truth.
            if (!_cfg.projectileMode)
            {
                for (int i = 0; i < target.Plugins.Count; i++)
                    if (target.Plugins[i] is IHealth hp) { hp.ApplyDamage(damage); break; }
            }

            _cooldown = 1f / Mathf.Max(0.05f, _cfg.fireRate);
            return true;
        }
    }
}
