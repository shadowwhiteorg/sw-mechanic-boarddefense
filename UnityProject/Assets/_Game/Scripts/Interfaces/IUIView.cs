namespace _Game.Interfaces
{
    public interface IUIView
    {
        void Bind(IUIModel model);
        void Unbind();
        void Show();
        void Hide();
    }
}