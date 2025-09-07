using UnityEngine;

namespace _Game.Interfaces
{
    public interface ICharacterView
    {
        Transform Root { get; }
        void Bind(object ctx);
        void Show();
        void Hide();
        void SetGhostVisual(bool isGhost, bool valid);
    }

}