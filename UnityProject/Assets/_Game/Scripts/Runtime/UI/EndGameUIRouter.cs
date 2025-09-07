// Assets/_Game/Scripts/Runtime/UI/EndGameUIRouter.cs
using UnityEngine;
using _Game.Interfaces;    // IEventBus
using _Game.Core.Events;   // GameWonEvent, GameLostEvent

namespace _Game.Runtime.Systems.UI
{
    /// <summary>
    /// Listens for GameWonEvent / GameLostEvent.
    /// On end: pauses the game, hides optional gameplay root, and shows the configured screen via UiService.
    /// </summary>
    public sealed class EndGameUIRouter : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private MonoBehaviour eventBusBehaviour; // must implement IEventBus
        [SerializeField] private UiService uiService;

        [Header("Screens (Prefabs)")]
        [SerializeField] private GameObject winScreenPrefab;
        [SerializeField] private GameObject loseScreenPrefab;

        [Header("Behavior")]
        [SerializeField] private Transform hideOnEnd;   // e.g., selectionModelsParent (optional)
        [SerializeField] private bool pauseAudio = true;
        [SerializeField] private bool hideAllBeforeShow = true;

        private IEventBus _bus;
        private bool _ended;

        private void Awake()
        {
            _bus = eventBusBehaviour as IEventBus;
            if (_bus == null)
                Debug.LogError("[EndGameUIRouter] eventBusBehaviour must implement IEventBus.");

            if (uiService == null)
                Debug.LogError("[EndGameUIRouter] UiService reference is missing.");
        }

        private void OnEnable()
        {
            if (_bus == null) return;
            _bus.Subscribe<GameWonEvent>(OnWin);
            _bus.Subscribe<GameLostEvent>(OnLose);
        }

        private void OnDisable()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<GameWonEvent>(OnWin);
            _bus.Unsubscribe<GameLostEvent>(OnLose);
        }

        private void OnWin(GameWonEvent _)
        {
            ShowEndScreen(win: true);
        }

        private void OnLose(GameLostEvent _)
        {
            ShowEndScreen(win: false);
        }

        private void ShowEndScreen(bool win)
        {
            if (_ended) return;
            _ended = true;

            // Pause simulation
            Time.timeScale = 0f;
            if (pauseAudio) AudioListener.pause = true;

            // Hide any interactive gameplay root (optional)
            if (hideOnEnd) hideOnEnd.gameObject.SetActive(false);

            if (uiService == null) return;

            if (hideAllBeforeShow) uiService.HideAll();
            var prefab = win ? winScreenPrefab : loseScreenPrefab;
            uiService.Show(prefab);
        }
    }
}
