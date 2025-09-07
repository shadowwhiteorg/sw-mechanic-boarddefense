using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Systems.UI.Lose
{
    public class LoseUIView : BaseUIView
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button retryButton;

        public event System.Action OnRetryClicked;

        protected override void OnBind()
        {
            retryButton.onClick.AddListener(() => OnRetryClicked?.Invoke());
        }

        protected override void OnViewUpdated()
        {
            var m = (LoseUIModel)Model;
            messageText.text = m.Message;
        }
    }
}