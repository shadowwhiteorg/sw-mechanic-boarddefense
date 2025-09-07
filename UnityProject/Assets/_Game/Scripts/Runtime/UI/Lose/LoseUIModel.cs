namespace _Game.Systems.UI.Lose
{
    public class LoseUIModel : BaseUIModel
    {
        public string Message { get; private set; }

        public void SetMessage(string message)
        {
            Message = message;
            NotifyUpdated();
        }
    }
}