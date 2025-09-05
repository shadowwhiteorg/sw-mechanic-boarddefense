using UnityEngine;
using _Game.Runtime.Characters.Config;

namespace _Game.Runtime.Selection
{
    /// <summary>
    /// Simple holder for a 3D selection model:
    /// - Stores its archetype and initial position
    /// - Provides TryRaycastBounds(Ray, out t) using Renderer.bounds (no colliders)
    /// </summary>
    public class SelectableCharacterView : MonoBehaviour
    {
        [Header("Optional: override auto-detection")]
        [SerializeField] private Renderer[] renderers;

        private CharacterArchetype _archetype;
        private Vector3 _initialPosition;

        public void Initialize(CharacterArchetype archetype)
        {
            _archetype = archetype;
            _initialPosition = transform.position;

            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(includeInactive: false);
        }

        public void ResetPosition()
        {
            transform.position = _initialPosition;
        }

        /// <summary>
        /// Intersects the pointer ray with world-space AABBs of this selectable's renderers.
        /// Returns true if any renderer bounds is hit; t is the nearest hit distance.
        /// </summary>
        public bool TryRaycastBounds(Ray ray, out float t)
        {
            t = float.PositiveInfinity;
            bool hit = false;

            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(includeInactive: false);

            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (!r || !r.enabled || !r.gameObject.activeInHierarchy) continue;

#if UNITY_2021_2_OR_NEWER
                if (r.bounds.IntersectRay(ray, out float localT))
#else
                if (IntersectRayAABB(ray, r.bounds, out float localT))
#endif
                {
                    if (localT < t)
                    {
                        t = localT;
                        hit = true;
                    }
                }
            }

            return hit;
        }

#if !UNITY_2021_2_OR_NEWER
        // Fallback for older Unity versions that lack Bounds.IntersectRay(out t)
        private static bool IntersectRayAABB(Ray ray, Bounds b, out float t)
        {
            t = 0f;

            Vector3 dir = ray.direction;
            // Avoid div by zero; small epsilon to keep sign
            Vector3 inv = new Vector3(
                1f / (Mathf.Abs(dir.x) < 1e-6f ? Mathf.Sign(dir.x) * 1e-6f : dir.x),
                1f / (Mathf.Abs(dir.y) < 1e-6f ? Mathf.Sign(dir.y) * 1e-6f : dir.y),
                1f / (Mathf.Abs(dir.z) < 1e-6f ? Mathf.Sign(dir.z) * 1e-6f : dir.z));

            Vector3 t1 = (b.min - ray.origin) * inv;
            Vector3 t2 = (b.max - ray.origin) * inv;

            float tmin = Mathf.Max(Mathf.Min(t1.x, t2.x), Mathf.Min(t1.y, t2.y), Mathf.Min(t1.z, t2.z));
            float tmax = Mathf.Min(Mathf.Max(t1.x, t2.x), Mathf.Max(t1.y, t2.y), Mathf.Max(t1.z, t2.z));

            if (tmax < 0f || tmin > tmax) return false;
            t = tmin >= 0f ? tmin : tmax; // if origin inside box, take exit
            return t >= 0f;
        }
#endif

        public CharacterArchetype Archetype => _archetype;
    }
}
