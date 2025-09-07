using _Game.Enums;
using _Game.Interfaces;
using _Game.Utils;
using UnityEngine;

namespace _Game.Systems.UI
{
    public abstract class BaseUIView : MonoBehaviour, IUIView
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        protected BaseUIModel Model { get; private set; }

        public virtual void Bind(IUIModel model)
        {
            Model = model as BaseUIModel;
            if (Model == null)
            {
                Debug.LogError("Model must inherit from BaseUIModel");
                return;
            }

            Model.OnUpdated += OnViewUpdated;

            OnBind();
            OnViewUpdated();
        }

        public virtual void Unbind()
        {
            if (Model != null)
            {
                Model.OnUpdated -= OnViewUpdated;
            }
        }

        public virtual void Show()
        {
            if (_canvasGroup != null)
            {
                Tween.Float(value => _canvasGroup.alpha = value, _canvasGroup.alpha, 1f, 1f, Ease.Linear, () =>
                {
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                });
            }
        }

        public virtual void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        protected virtual void OnBind() { }

        protected abstract void OnViewUpdated();
    }
}