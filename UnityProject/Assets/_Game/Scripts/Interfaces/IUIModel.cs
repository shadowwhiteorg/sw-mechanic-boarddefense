using System;

namespace _Game.Interfaces
{
    public interface IUIModel
    {
        event Action OnUpdated;
        bool IsActive { get; }
        void SetActive(bool active);
    }
}