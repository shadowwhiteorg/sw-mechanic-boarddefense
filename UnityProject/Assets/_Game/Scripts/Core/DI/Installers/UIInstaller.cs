using UnityEngine;
using _Game.Interfaces;
using _Game.Systems.UI.Lose;
using _Game.Systems.UI.Win;
using _Game.Systems.UISystem.Screens;
using _Game.Systems.UISystem.Views;

namespace _Game.Core.DI
{
    public class UIInstaller : MonoBehaviour
    {
        [Header("UI References")] 
        [SerializeField] private WinUIScreen winUIScreenPrefab;
        [SerializeField] private LoseUIScreen loseUIScreenPrefab;
        [SerializeField] private Canvas canvasRoot;

        public void Initialize(DIContainer container, IEventBus eventBus)
        {
            InstallWinUI(container, eventBus);
            InstallLoseUI(container, eventBus);
        }

        private void InstallWinUI(DIContainer container, IEventBus eventBus)
        {
            var model = new WinUIModel();
            container.BindSingleton(model);
            var screenGO = Instantiate(winUIScreenPrefab, canvasRoot.transform);
            var view = screenGO.GetComponentInChildren<WinUIView>();
            var screen = screenGO.GetComponent<WinUIScreen>();
            screen.Construct(model, view, eventBus);
        }
        private void InstallLoseUI(DIContainer container, IEventBus eventBus)
        {
            var model = new LoseUIModel();
            container.BindSingleton(model);
            var screenGO = Instantiate(loseUIScreenPrefab, canvasRoot.transform);
            var view = screenGO.GetComponentInChildren<LoseUIView>();
            var screen = screenGO.GetComponent<LoseUIScreen>();
            screen.Construct(model, view, eventBus);
        }
        
        
    }
}
