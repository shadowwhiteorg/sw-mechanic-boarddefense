using _Game.Interfaces;
using UnityEngine;

namespace _Game.Core.Events
{
    public readonly struct AttackPerformedEvent : IGameEvent
    {
        public readonly int sourceId;
        public readonly int targetId;        // -1 for non-targeted AoE
        public readonly Vector3 sourceWorld; // muzzle position
        public readonly Vector3 targetWorld; // initial aim point (target's current pos)
        public readonly int damage;
        public readonly bool projectileMode; // true => projectile will deliver damage
        public readonly float projectileSpeed;
        public readonly int pierceCount;     // 0 = no pierce
        public readonly float splashRadius;  // 0 = no splash

        public AttackPerformedEvent(
            int sourceId, int targetId, Vector3 sourceWorld, Vector3 targetWorld,
            int damage, bool projectileMode, float projectileSpeed, int pierceCount, float splashRadius)
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
        }
    }

    /// Raised on impact (for VFX/SFX/UI).
    public readonly struct ProjectileHitEvent : IGameEvent
    {
        public readonly int projectileInstanceId;
        public readonly int targetId;
        public readonly int damage;
        public readonly Vector3 hitWorld;

        public ProjectileHitEvent(int projectileInstanceId, int targetId, int damage, Vector3 hitWorld)
        {
            this.projectileInstanceId = projectileInstanceId;
            this.targetId = targetId;
            this.damage = damage;
            this.hitWorld = hitWorld;
        }
    }
}