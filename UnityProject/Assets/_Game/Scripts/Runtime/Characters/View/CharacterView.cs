using _Game.Interfaces;
using UnityEngine;

namespace _Game.Runtime.Characters.View
{
    [DisallowMultipleComponent]
    public sealed class CharacterView : MonoBehaviour, ICharacterView
    {
        public Transform Root
        {
            get
            {
                if (this == null) return null;
                var t = transform;        
                return t ? t : null;
            }
        }

        public void Bind(object ctx) {}
        public void Show() { if (this && Root) Root.gameObject.SetActive(true); }
        public void Hide() { if (this && Root) Root.gameObject.SetActive(false); }

        public void SetGhostVisual(bool isGhost, bool valid)
        {
            if (!this || !Root) return;
        }
    }
}