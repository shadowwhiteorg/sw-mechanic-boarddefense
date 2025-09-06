using UnityEngine;

namespace _Game.Runtime.Combat
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Game/Combat/WeaponConfig")]
    public sealed class WeaponConfig : ScriptableObject
    {
        [Header("Cadence & Range")]
        [Min(0.05f)] public float fireRate = 1.0f; // shots/sec
        [Min(0)]     public int   rangeBlocks = 4;
        public bool   projectileMode = true; // false => hitscan using projectileConfig.damage

        [Header("Projectile")]
        public ProjectileConfig projectileConfig;

        [Header("View")]
        public Vector3 muzzleOffset = Vector3.zero;
    }
}