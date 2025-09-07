using UnityEngine;

namespace _Game.Runtime.Combat
{
    [CreateAssetMenu(fileName = "ProjectileConfig", menuName = "Game/Combat/ProjectileConfig")]
    public sealed class ProjectileConfig : ScriptableObject
    {
        [Header("Prefab & VFX")]
        [Tooltip("Must contain the mesh/model; can optionally include trail & hit particle objects as children.")]
        public GameObject projectilePrefab;

        [Header("Gameplay")]
        [Min(1)]     public int   damage        = 1;
        [Min(0.1f)]  public float speed         = 8f;
        [Min(0)]     public int   pierceCount   = 0;
        [Min(0f)]    public float splashRadius  = 0f;
    }
}