using _Game.Core.Events;
using _Game.Systems.UI;
using _Game.Systems.UI.Win;
using _Game.Systems.UISystem.Views;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Game.Systems.UISystem.Screens
{
    public sealed class WinUIScreen : BaseUIScreen<WinUIModel, WinUIView>
    {
        public override void Construct(WinUIModel model, WinUIView view, _Game.Interfaces.IEventBus eventBus)
        {
            base.Construct(model, view, eventBus);

            // Wire view actions
            View.OnRestart = () => { Unpause(); SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); };
            View.OnNext    = () => { Unpause(); /* your next-level call here */ SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); };
            View.OnQuit    = () => { Unpause(); /* load menu or quit */ };

            // Listen to game events
            EventBus.Subscribe<GameWonEvent>(_ =>
            {
                Pause();
                Show();
            });
        }

        private static void Pause()  { Time.timeScale = 0f; AudioListener.pause = true; }
        private static void Unpause(){ Time.timeScale = 1f; AudioListener.pause = false; }
    }
}