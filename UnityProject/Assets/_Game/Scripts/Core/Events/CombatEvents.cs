using _Game.Interfaces;
using _Game.Runtime.Combat;
using UnityEngine;

namespace _Game.Core.Events
{
    public readonly struct AttackPerformedEvent : IGameEvent
    {
        public readonly int sourceId;
        public readonly int targetId;        // -1 for non-targeted AoE
        public readonly Vector3 sourceWorld; // muzzle position
        public readonly Vector3 targetWorld; // aim point at fire time

        // Damage & flight data (from ProjectileConfig)
        public readonly int damage;
        public readonly bool projectileMode;
        public readonly float projectileSpeed;
        public readonly int pierceCount;
        public readonly float splashRadius;

        // Optional reference for systems that want prefab/VFX info
        public readonly ProjectileConfig projectileConfig;

        public AttackPerformedEvent(
            int sourceId, int targetId, Vector3 sourceWorld, Vector3 targetWorld,
            int damage, bool projectileMode, float projectileSpeed, int pierceCount, float splashRadius,
            ProjectileConfig projectileConfig)
        {
            this.sourceId = sourceId;
            this.targetId = targetId;
            this.sourceWorld = sourceWorld;
            this.targetWorld = targetWorld;
            this.damage = damage;
            this.projectileMode = projectileMode;
            this.projectileSpeed = projectileSpeed;
            this.pierceCount = pierceCount;
            this.splashRadius = splashRadius;
            this.projectileConfig = projectileConfig;
        }
    }

    /// Raised on impact (for VFX/SFX/UI).
    public readonly struct ProjectileHitEvent : IGameEvent
    {
        public readonly int ProjectileInstanceId;
        public readonly int TargetId;
        public readonly int Damage;
        public readonly Vector3 HitWorld;

        public ProjectileHitEvent(int projectileInstanceId, int targetId, int damage, Vector3 hitWorld)
        {
            this.ProjectileInstanceId = projectileInstanceId;
            this.TargetId = targetId;
            this.Damage = damage;
            this.HitWorld = hitWorld;
        }
    }
    
    public readonly struct EnemyReachedBaseEvent : IGameEvent
    {
        public readonly int EnemyId;
        public EnemyReachedBaseEvent(int enemyId) { EnemyId = enemyId; }
        public override string ToString() => $"EnemyReachedBase(EnemyId={EnemyId})";
    }

    public readonly struct BaseDamagedEvent : IGameEvent
    {
        public readonly int Amount;
        public readonly int NewHp;
        public BaseDamagedEvent(int amount, int newHp) { Amount = amount; NewHp = newHp; }
        public override string ToString() => $"BaseDamaged(Amount={Amount}, NewHp={NewHp})";
    }
    
    
}