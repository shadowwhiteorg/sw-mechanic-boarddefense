using UnityEngine;

namespace _Game.Interfaces
{
    public interface ICharacterView
    {
        Transform Root { get; }
        void Bind(object ctx);
        void Show();
        void Hide();
        /// isGhost: use a ghost material; valid: green/red tint etc.
        void SetGhostVisual(bool isGhost, bool valid);
    }

}