using _Game.Core.Events;
using _Game.Core.Constants;
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

            eventBus.Subscribe<GameWonEvent>(e =>
            {
                model.SetMessage(PlayerPrefs.GetInt(GameConstants.PlayerPrefsLevel,1).ToString());
                Show();
            });

            view.OnNextClicked  += () => eventBus.Fire(new NextLevelEvent());
            
        }
    }
}