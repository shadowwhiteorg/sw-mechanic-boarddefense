using _Game.Interfaces;
using UnityEngine;

namespace _Game.Runtime.Characters.View
{
    [DisallowMultipleComponent]
    public sealed class CharacterView : MonoBehaviour, ICharacterView
    {
        [SerializeField] private SpriteRenderer[] tintables;

        public Transform Root => transform;

        public void Bind(object model) { /* optional visual binding */ }
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public void SetGhostVisual(bool ghost, bool valid)
        {
            if (tintables == null) return;
            var a = ghost ? 0.6f : 1f;
            var baseCol = valid ? Color.white : new Color(.4f, 0.15f, 0.35f, 1f);
            foreach (var r in tintables)
            {
                if (!r) continue;
                var c = baseCol; c.a = a;
                r.color = c;
            }
        }
    }
}