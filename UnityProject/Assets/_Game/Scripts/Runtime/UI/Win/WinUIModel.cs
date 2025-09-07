// Assets/_Game/Scripts/Systems/UISystem/Models/WinUIModel.cs
using _Game.Systems.UI;

namespace _Game.Systems.UI.Win
{
    public sealed class WinUIModel : BaseUIModel
    {
        public string Title { get; private set; } = "Victory!";
        public string Subtitle { get; private set; } = "You cleared the level.";

        public void SetTexts(string title, string subtitle)
        {
            if (!string.IsNullOrEmpty(title))    Title    = title;
            if (!string.IsNullOrEmpty(subtitle)) Subtitle = subtitle;
            NotifyUpdated();
        }
    }
}