// Assets/_Game/Scripts/Systems/UISystem/Models/LoseUIModel.cs
using _Game.Systems.UI;

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