using _Game.Interfaces;
using UnityEngine;

namespace _Game.Runtime.Characters.View
{
    [DisallowMultipleComponent]
    public sealed class CharacterView : MonoBehaviour, ICharacterView
    {
        // Robust Root: returns null if this component or its GameObject is destroyed
        public Transform Root
        {
            get
            {
                // Unity’s fake-null: destroyed UnityEngine.Object compares == null
                if (this == null) return null;
                var t = transform;         // will itself throw if component is in a weird state
                return t ? t : null;
            }
        }

        public void Bind(object ctx) { /* optional: assign animator/materials, etc. */ }
        public void Show() { if (this && Root) Root.gameObject.SetActive(true); }
        public void Hide() { if (this && Root) Root.gameObject.SetActive(false); }

        public void SetGhostVisual(bool isGhost, bool valid)
        {
            // Optional: plug in material swaps or color tints here.
            // Keep safe:
            if (!this || !Root) return;
            // Example placeholder: do nothing by default.
        }
    }
}