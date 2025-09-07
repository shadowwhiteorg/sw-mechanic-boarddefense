// Assets/_Game/Scripts/Runtime/Selection/SelectableCharacterView.cs
using UnityEngine;
using _Game.Runtime.Characters.Config;

namespace _Game.Runtime.Selection
{
    /// <summary>
    /// Lightweight, click/drag-friendly selectable view used on the selection row.
    /// - Stores its spawn "slot" (InitialPosition) so we can reset or refill at the exact same spot.
    /// - Raycasts against its renderers' bounds (no collider required).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SelectableCharacterView : MonoBehaviour
    {
        [Header("Optional: override auto-detection")]
        [SerializeField] private Renderer[] renderers;

        private CharacterArchetype _archetype;
        private Vector3 _initialPosition;

        public CharacterArchetype Archetype => _archetype;
        public Vector3 InitialPosition => _initialPosition;

        /// <summary>Call immediately after instantiation to set archetype and cache initial slot.</summary>
        public void Initialize(CharacterArchetype archetype)
        {
            _archetype = archetype;
            _initialPosition = transform.position;

            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(includeInactive: false);
        }

        /// <summary>Resets this view back to the slot where it was spawned.</summary>
        public void ResetPosition()
        {
            transform.position = _initialPosition;
        }

        /// <summary>
        /// Returns true if the given ray intersects any of this view's renderer bounds.
        /// 't' is the distance along the ray to the nearest hit.
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
                if (!r || !r.enabled || !r.gameObject.activeInHierarchy)
                    continue;

                if (r.bounds.IntersectRay(ray, out float localT))
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

        /// <summary>Optional hook to toggle any outline/ghost visuals.</summary>
        public void SetAsSelectable(bool on)
        {
            // Intentionally minimal; customize if you use outlines/ghost materials.
            if (this && gameObject) gameObject.SetActive(true);
        }
    }
}
