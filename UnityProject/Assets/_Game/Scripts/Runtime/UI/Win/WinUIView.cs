using _Game.Systems.UI;
using _Game.Systems.UI.Win;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Game.Systems.UISystem.Views
{
    public class WinUIView : BaseUIView
    {
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private Button          nextButton;

        public event System.Action OnNextClicked;

        protected override void OnBind()
        {
            nextButton .onClick.AddListener(() => OnNextClicked?.Invoke());
        }

        protected override void OnViewUpdated()
        {
            var m = (WinUIModel)Model;
            levelNumberText.text = m.Message;
        }
    }
}