using UnityEngine;
using _Game.Runtime.Characters.Config;

namespace _Game.Runtime.Selection
{

    public class SelectableCharacterView : MonoBehaviour
    {
        [Header("Optional: override auto-detection")] [SerializeField]
        private Renderer[] renderers;

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
        public CharacterArchetype Archetype => _archetype;
    }
}
