// Assets/_Game/Scripts/Systems/UISystem/Models/LoseUIModel.cs
using _Game.Systems.UI;

namespace _Game.Systems.UI.Lose
{
    public sealed class LoseUIModel : BaseUIModel
    {
        public string Title { get; private set; } = "Defeat";
        public string Subtitle { get; private set; } = "Try a different strategy.";

        public void SetTexts(string title, string subtitle)
        {
            if (!string.IsNullOrEmpty(title))    Title    = title;
            if (!string.IsNullOrEmpty(subtitle)) Subtitle = subtitle;
            NotifyUpdated();
        }
    }
}