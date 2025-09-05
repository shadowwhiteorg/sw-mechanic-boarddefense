using _Game.Enums;
using _Game.Interfaces;
using UnityEngine;
using _Game.Runtime.Combat;

namespace _Game.Runtime.Characters.Plugins
{
    public sealed class RangedAttackPlugin : IAttack
    {
        private readonly TargetingService _targets;
        private readonly float _ratePerSec;
        private readonly int _damage;
        private readonly int _rangeBlocks;
        private readonly AttackDirection _direction;

        private CharacterEntity _self;
        private float _cooldown;

        public RangedAttackPlugin(
            TargetingService targets, float ratePerSec, int damage, int rangeBlocks, AttackDirection direction)
        {
            _targets = targets;
            _ratePerSec = Mathf.Max(0.05f, ratePerSec);
            _damage = Mathf.Max(1, damage);
            _rangeBlocks = Mathf.Max(0, rangeBlocks);
            _direction = direction;
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

            // Acquire target
            if (TryAcquire(out var t) && TryGetHealth(t, out var hp))
            {
                hp.ApplyDamage(_damage);
                _cooldown = 1f / _ratePerSec;
            }
            else
            {
                // no target; small retry delay
                _cooldown = 0.1f;
            }
        }

        private bool TryAcquire(out CharacterEntity target)
        {
            if (_direction == AttackDirection.Forward)
                return _targets.TryFindForwardEnemy(_self, _rangeBlocks, out target);

            return _targets.TryFindOmniEnemy(_self, _rangeBlocks, out target);
        }

        private static bool TryGetHealth(CharacterEntity e, out IHealth hp)
        {
            for (int i = 0; i < e.Plugins.Count; i++)
            {
                if (e.Plugins[i] is IHealth h) { hp = h; return true; }
            }
            hp = null; return false;
        }
    }
}
