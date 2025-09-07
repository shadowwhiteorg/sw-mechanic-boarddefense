using _Game.Systems.UI;
using _Game.Systems.UI.Win;
using TMPro;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

namespace _Game.Systems.UISystem.Views
{
    public sealed class WinUIView : BaseUIView
    {

        public TextMeshProUGUI titleText;
        public TextMeshProUGUI subtitleText;
        public Button restartButton;
        public Button nextButton;
        public Button quitButton;

        public System.Action OnRestart, OnNext, OnQuit;

        protected override void OnViewUpdated()
        {
            var m = (WinUIModel)Model;
            if (titleText)    titleText.text    = m.Title;
            if (subtitleText) subtitleText.text = m.Subtitle;
        }

        protected override void OnBind()
        {
            if (restartButton) { restartButton.onClick.RemoveAllListeners(); restartButton.onClick.AddListener(()=>OnRestart?.Invoke()); }
            if (nextButton)    { nextButton.onClick.RemoveAllListeners();    nextButton.onClick.AddListener(()=>OnNext?.Invoke());    }
            if (quitButton)    { quitButton.onClick.RemoveAllListeners();    quitButton.onClick.AddListener(()=>OnQuit?.Invoke());    }
        }
    }
}