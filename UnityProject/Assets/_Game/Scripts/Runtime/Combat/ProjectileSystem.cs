using System.Collections.Generic;
using _Game.Core.Events;
using UnityEngine;
using _Game.Interfaces;
using _Game.Runtime.Characters;
using _Game.Utils;

namespace _Game.Runtime.Combat
{
    /// Listens for AttackPerformedEvent (projectileMode) and drives projectiles.
    /// Uses GameObjectPool. Applies damage on impact; emits ProjectileHitEvent.
    public sealed class ProjectileSystem : IUpdatableSystem
    {
        private readonly IEventBus _bus;
        private readonly CharacterRepository _repo;
        private readonly GameObjectPool _pool;
        private readonly List<Projectile> _live = new();

        public ProjectileSystem(IEventBus bus, CharacterRepository repo, GameObjectPool pool)
        {
            _bus = bus; _repo = repo; _pool = pool;
            _bus.Subscribe<AttackPerformedEvent>(OnAttack);
        }

        public void Tick()
        {
            float dt = Time.deltaTime;

            for (int i = _live.Count - 1; i >= 0; i--)
            {
                var p = _live[i];

                // homing to current target position (if still alive)
                Vector3 targetPos;
                if (_repo.TryGetById(p.TargetId, out var target))
                    targetPos = target.View.Root.position;
                else
                    targetPos = p.transform.position; // lost target -> stop (optional: despawn)

                var to = targetPos - p.transform.position;
                float dist = to.magnitude;
                if (dist <= 0.05f)
                {
                    Impact(p, target, targetPos);
                    if (p.Pierce <= 0) Despawn(i);
                    else p.Pierce--;
                    continue;
                }

                if (dist > 0.0001f) to /= dist;
                p.transform.position += to * (p.Speed * dt);
            }
        }

        private void OnAttack(AttackPerformedEvent e)
        {
            if (!e.projectileMode) return;

            var go = _pool.Get();
            var projectile = go.GetComponent<Projectile>() ?? go.AddComponent<Projectile>();
            projectile.Init(e.sourceId, e.targetId, e.sourceWorld, e.damage, e.projectileSpeed, e.pierceCount, e.splashRadius);
            _live.Add(projectile);
        }

        private void Impact(Projectile p, CharacterEntity target, Vector3 hitWorld)
        {
            // direct hit
            if (target != null)
            {
                for (int i = 0; i < target.Plugins.Count; i++)
                    if (target.Plugins[i] is IHealth hp) { hp.ApplyDamage(p.Damage); break; }
            }

            // splash AoE (optional — keep simple / grid-based if you add it)
            if (p.Splash > 0f)
            {
                // TODO: iterate repo by nearby cells and apply reduced damage
            }

            _bus.Fire(new ProjectileHitEvent(p.GetInstanceID(), p.TargetId, p.Damage, hitWorld));
        }

        private void Despawn(int liveIndex)
        {
            var prj = _live[liveIndex];
            _live.RemoveAt(liveIndex);
            prj.gameObject.SetActive(false);
            _pool.Return(prj.gameObject);
        }
    }
}
