using _Game.Interfaces;
using _Game.Runtime.Combat;
using UnityEngine;

namespace _Game.Core.Events
{
    public readonly struct AttackPerformedEvent : IGameEvent
    {
        public readonly int SourceId;
        public readonly int TargetId;        
        public readonly Vector3 SourceWorld; 
        public readonly Vector3 TargetWorld; 

        public readonly int Damage;
        public readonly bool ProjectileMode;
        public readonly float ProjectileSpeed;
        public readonly int PierceCount;
        public readonly float SplashRadius;

        public readonly ProjectileConfig ProjectileConfig;

        public AttackPerformedEvent(
            int sourceId, int targetId, Vector3 sourceWorld, Vector3 targetWorld,
            int damage, bool projectileMode, float projectileSpeed, int pierceCount, float splashRadius,
            ProjectileConfig projectileConfig)
        {
            SourceId = sourceId;
            TargetId = targetId;
            SourceWorld = sourceWorld;
            TargetWorld = targetWorld;
            Damage = damage;
            ProjectileMode = projectileMode;
            ProjectileSpeed = projectileSpeed;
            PierceCount = pierceCount;
            SplashRadius = splashRadius;
            ProjectileConfig = projectileConfig;
        }
    }

    public readonly struct ProjectileHitEvent : IGameEvent
    {
        public readonly int ProjectileInstanceId;
        public readonly int TargetId;
        public readonly int Damage;
        public readonly Vector3 HitWorld;

        public ProjectileHitEvent(int projectileInstanceId, int targetId, int damage, Vector3 hitWorld)
        {
            ProjectileInstanceId = projectileInstanceId;
            TargetId = targetId;
            Damage = damage;
            HitWorld = hitWorld;
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
    
    public readonly struct BaseHealthChangedEvent : IGameEvent
    {
        public readonly int Current;
        public readonly int Max;
        public BaseHealthChangedEvent(int current, int max) { Current = current; Max = max; }
    }
    
    
}