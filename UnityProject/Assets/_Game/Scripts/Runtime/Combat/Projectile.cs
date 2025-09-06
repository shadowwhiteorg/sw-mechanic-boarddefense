using UnityEngine;

namespace _Game.Runtime.Combat
{
    /// Pooled projectile. Simple data container; motion handled by ProjectileSystem.
    public sealed class Projectile : MonoBehaviour
    {
        public int SourceId   { get; private set; }
        public int TargetId   { get; private set; }
        public int Damage     { get; private set; }
        public float Speed    { get; private set; }
        public int Pierce     { get; set; }
        public float Splash   { get; private set; }

        public void Init(int sourceId, int targetId, Vector3 start, int damage, float speed, int pierce, float splash)
        {
            SourceId = sourceId;
            TargetId = targetId;
            Damage   = damage;
            Speed    = speed;
            Pierce   = Mathf.Max(0, pierce);
            Splash   = Mathf.Max(0f, splash);
            transform.position = start;
            gameObject.SetActive(true);
        }
    }
}