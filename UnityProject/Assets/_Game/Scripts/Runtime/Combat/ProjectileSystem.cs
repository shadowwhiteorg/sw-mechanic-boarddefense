using System.Collections.Generic;
using _Game.Core.Events;
using UnityEngine;
using _Game.Interfaces;
using _Game.Runtime.Characters;
using _Game.Utils;

namespace _Game.Runtime.Combat
{
    public sealed class ProjectileSystem : IUpdatableSystem
    {
        private readonly IEventBus _bus;
        private readonly CharacterRepository _repo;

        private readonly GameObjectPool _fallbackPool;

        private readonly List<(Projectile projectile, GameObjectPool pool)> _live = new();

        public ProjectileSystem(IEventBus bus, CharacterRepository repo, GameObjectPool fallbackPool)
        {
            _bus = bus; _repo = repo; _fallbackPool = fallbackPool;
            _bus.Subscribe<AttackPerformedEvent>(OnAttack);
        }

        public void Tick()
        {
            float dt = Time.deltaTime;

            for (int i = _live.Count - 1; i >= 0; i--)
            {
                var (p, pool) = _live[i];

                Vector3 targetPos;
                CharacterEntity target = null;
                if (_repo.TryGetById(p.TargetId, out target) && target?.View?.Root)
                    targetPos = target.View.Root.position;
                else
                    targetPos = p.transform.position;

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
            if (!e.ProjectileMode) return;

            GameObjectPool pool = _fallbackPool;
            GameObject go;

            if (e.ProjectileConfig && e.ProjectileConfig.projectilePrefab)
            {
                pool = new GameObjectPool(e.ProjectileConfig.projectilePrefab, initialSize: 8, parent: null);
            }

            go = pool.Get();
            var projectile = go.GetComponent<Projectile>() ?? go.AddComponent<Projectile>();
            projectile.Init(e.SourceId, e.TargetId, e.SourceWorld, e.Damage, e.ProjectileSpeed, e.PierceCount, e.SplashRadius);
            _live.Add((projectile, pool));
        }

        private void Impact(Projectile p, CharacterEntity target, Vector3 hitWorld)
        {
            if (target != null)
            {
                for (int i = 0; i < target.Plugins.Count; i++)
                    if (target.Plugins[i] is IHealth hp) { hp.ApplyDamage(p.Damage); break; }
            }

            // TODO: splash AoE if p.Splash > 0f (grid-based ring)
            _bus.Fire(new ProjectileHitEvent(p.GetInstanceID(), p.TargetId, p.Damage, hitWorld));
        }

        private void Despawn(int liveIndex)
        {
            var (prj, pool) = _live[liveIndex];
            _live.RemoveAt(liveIndex);
            prj.gameObject.SetActive(false);
            pool.Return(prj.gameObject);
        }
    }
}
