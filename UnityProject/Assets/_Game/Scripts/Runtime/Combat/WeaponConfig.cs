using UnityEngine;

namespace _Game.Runtime.Combat
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Game/Combat/WeaponConfig")]
    public sealed class WeaponConfig : ScriptableObject
    {
        [Header("Core")]
        [Min(0.05f)] public float fireRate = 1.0f; // shots/sec
        [Min(1)]     public int   damage   = 1;
        [Min(0)]     public int   rangeBlocks = 4;
        public bool projectileMode = true;

        [Header("Projectile")]
        public GameObject projectilePrefab;
        [Min(0.1f)] public float projectileSpeed = 8f;
        [Min(0)]     public int   pierceCount = 0;
        [Min(0)]     public float splashRadius = 0f;

        [Header("View")]
        public Vector3 muzzleOffset = Vector3.zero;
    }
}