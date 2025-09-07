// Assets/_Game/Scripts/Systems/UISystem/Models/WinUIModel.cs
using _Game.Systems.UI;

namespace _Game.Systems.UI.Win
{
    public class WinUIModel : BaseUIModel
    {
        public string Message { get; private set; }

        public void SetMessage(string message)
        {
            Message = message;
            NotifyUpdated();
        }
    }
}