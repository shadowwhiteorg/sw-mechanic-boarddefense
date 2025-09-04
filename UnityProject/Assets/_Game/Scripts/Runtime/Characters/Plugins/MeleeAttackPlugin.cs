using _Game.Interfaces;
using UnityEngine;

namespace _Game.Runtime.Characters.Plugins
{
    /// <summary>
    /// Minimal melee placeholder (cooldown only). Wire your own target acquisition and damage calls.
    /// </summary>
    public sealed class MeleeAttackPlugin : IAttack
    {
        private readonly float _rate;  // attacks per second
        private readonly int _damage;
        private float _cooldown;

        public MeleeAttackPlugin(float rate, int damage)
        {
            _rate = Mathf.Max(0.05f, rate);
            _damage = Mathf.Max(1, damage);
        }

        public void OnSpawn(CharacterEntity e) { _cooldown = 0f; }
        public void OnDespawn() { }

        public void Tick(float dt)
        {
            // Stub: you should add target search and call targetHealth.ApplyDamage(_damage).
            _cooldown -= dt;
            if (_cooldown <= 0f)
            {
                // if (target != null) target.ApplyDamage(_damage);
                _cooldown = 1f / _rate;
            }
        }
    }
}