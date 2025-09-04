using UnityEngine;

namespace _Game.Interfaces
{
    public interface ICharacterView
    {
        Transform Root { get; }
        void Bind(object model);
        void Show();
        void Hide();
        void SetGhostVisual(bool ghost, bool valid);
    }
}