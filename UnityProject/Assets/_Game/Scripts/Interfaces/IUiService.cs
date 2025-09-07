using _Game.Systems.UI;

namespace _Game.Interfaces
{
    public interface IUiService
    {
        /// <summary>Show a screen of type TScreen using the provided model instance.</summary>
        TScreen Show<TScreen, TModel>(TModel model)
            where TModel : BaseUIModel
            where TScreen : BaseUIScreen<TModel, BaseUIView>;

        /// <summary>Hide and (optionally) destroy all active screens.</summary>
        void HideAll(bool destroy = false);
    }
}