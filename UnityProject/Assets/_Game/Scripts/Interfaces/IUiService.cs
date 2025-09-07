using _Game.Systems.UI;

namespace _Game.Interfaces
{
    public interface IUiService
    {
        TScreen Show<TScreen, TModel>(TModel model)
            where TModel : BaseUIModel
            where TScreen : BaseUIScreen<TModel, BaseUIView>;

        void HideAll(bool destroy = false);
    }
}