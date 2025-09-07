using _Game.Core.Events;
using _Game.Interfaces;
using _Game.Systems.UI;
using _Game.Systems.UI.Win;
using _Game.Systems.UISystem.Views;
using UnityEngine;
namespace _Game.Systems.UISystem.Screens
{
    public class WinUIScreen : BaseUIScreen<WinUIModel, WinUIView>
    {
        public override void Construct(WinUIModel model, WinUIView view, IEventBus eventBus)
        {
            base.Construct(model, view, eventBus);

            // Show on level complete
            eventBus.Subscribe<GameWonEvent>(e =>
            {
                model.SetMessage(PlayerPrefs.GetInt("CurrentLevel",1).ToString());
                Show();
            });

            // Buttons
            view.OnNextClicked  += () => eventBus.Fire(new NextLevelEvent());
        }
    }
}