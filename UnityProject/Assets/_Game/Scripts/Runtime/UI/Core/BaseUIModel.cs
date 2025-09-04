using System;
using _Game.Interfaces;

namespace _Game.Systems.UISystem
{
    public abstract class BaseUIModel : IUIModel
    {
        public event Action OnUpdated;
        public bool IsActive { get; private set; }

        public virtual void SetActive(bool active)
        {
            if (IsActive == active) return;
            IsActive = active;
            NotifyUpdated();
        }

        protected void NotifyUpdated()
        {
            OnUpdated?.Invoke();
        }
    }
}