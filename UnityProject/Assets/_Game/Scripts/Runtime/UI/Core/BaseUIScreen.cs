using _Game.Interfaces;
using UnityEngine;

namespace _Game.Systems.UISystem
{
    public abstract class BaseUIScreen<TModel, TView> : MonoBehaviour, IUIScreen
        where TModel : BaseUIModel
        where TView : BaseUIView
    {
        protected TModel Model { get; private set; }
        protected TView View { get; private set; }
        protected IEventBus EventBus { get; private set; }

        public virtual void Construct(TModel model, TView view, IEventBus eventBus)
        {
            Model = model;
            View = view;
            EventBus = eventBus;

            View.Bind(Model);
        }

        public virtual void Show()
        {
            Model.SetActive(true);
            View.Show();
        }

        public virtual void Hide()
        {
            Model.SetActive(false);
            View.Hide();
        }
    }
}