// Assets/_Game/Scripts/Systems/UISystem/Screens/LoseUIScreen.cs
using _Game.Core.Events;
using _Game.Systems.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Game.Systems.UI.Lose
{
    public sealed class LoseUIScreen : BaseUIScreen<LoseUIModel, LoseUIView>
    {
        public override void Construct(LoseUIModel model, LoseUIView view, _Game.Interfaces.IEventBus eventBus)
        {
            base.Construct(model, view, eventBus);

            View.OnRestart = () => { Unpause(); SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); };
            View.OnQuit    = () => { Unpause(); /* load menu or quit */ };

            EventBus.Subscribe<GameLostEvent>(_ =>
            {
                Pause();
                Show();
            });
        }

        private static void Pause()  { Time.timeScale = 0f; AudioListener.pause = true; }
        private static void Unpause(){ Time.timeScale = 1f; AudioListener.pause = false; }
    }
}