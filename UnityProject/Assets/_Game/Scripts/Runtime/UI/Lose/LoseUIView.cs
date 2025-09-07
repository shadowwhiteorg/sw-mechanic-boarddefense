using TMPro;
using UnityEngine.UI;

namespace _Game.Systems.UI.Lose
{
    public sealed class LoseUIView : BaseUIView
    {
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI subtitleText;

        public Button restartButton;
        public Button quitButton;

        public System.Action OnRestart, OnQuit;

        protected override void OnViewUpdated()
        {
            var m = (LoseUIModel)Model;
            if (titleText)    titleText.text    = m.Title;
            if (subtitleText) subtitleText.text = m.Subtitle;
        }

        protected override void OnBind()
        {
            if (restartButton) { restartButton.onClick.RemoveAllListeners(); restartButton.onClick.AddListener(()=>OnRestart?.Invoke()); }
            if (quitButton)    { quitButton.onClick.RemoveAllListeners();    quitButton.onClick.AddListener(()=>OnQuit?.Invoke());    }
        }
    }
}