using UnityEngine;
using _Game.Runtime.Characters.Config;

namespace _Game.Runtime.Selection
{
    public class SelectableCharacterView : MonoBehaviour
    {
        [SerializeField] private Renderer[] boundingRenderers;
        private CharacterArchetype _archetype;

        public CharacterArchetype Archetype => _archetype;
        public event System.Action<SelectableCharacterView> OnClicked;
        
        public void Initialize(CharacterArchetype archetype)
        {
            _archetype = archetype;
        }

        public bool IsHovered { get; private set; }

        public void SetHovered(bool hovered)
        {
            if (IsHovered == hovered) return;
            IsHovered = hovered;

            // Optional: pulse scale or change material
            transform.localScale = hovered ? Vector3.one * 1.2f : Vector3.one;
        }

        public void Click()
        {
            if (_archetype == null)
            {
                Debug.LogError("SelectableCharacterView clicked, but archetype is null!");
                return;
            }
            OnClicked?.Invoke(this);
        }

        public bool ContainsScreenPoint(Camera cam, Vector2 screenPoint)
        {
            if (boundingRenderers == null || boundingRenderers.Length == 0)
                return false;

            Bounds combined = boundingRenderers[0].bounds;
            for (int i = 1; i < boundingRenderers.Length; i++)
                combined.Encapsulate(boundingRenderers[i].bounds);

            Vector3 min = cam.WorldToScreenPoint(combined.min);
            Vector3 max = cam.WorldToScreenPoint(combined.max);

            Rect screenRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            return screenRect.Contains(screenPoint);
        }
    }
}