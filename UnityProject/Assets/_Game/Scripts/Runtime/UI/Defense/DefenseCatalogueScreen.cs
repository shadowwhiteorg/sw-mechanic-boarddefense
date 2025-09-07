using _Game.Interfaces;                                
using _Game.Core.Events;                               

namespace _Game.Systems.UI.Defense
{
    public sealed class DefenseCatalogueScreen : BaseUIScreen<DefenseCatalogueModel, DefenseCatalogueView>
    {
        public override void Construct(DefenseCatalogueModel model, DefenseCatalogueView view, IEventBus eventBus)
        {
            base.Construct(model, view, eventBus);

            view.OnItemClicked += idx =>
            {
                if (idx < 0 || idx >= Model.Items.Count) return;
                var a = Model.Items[idx];
                Model.SetSelectedIndex(idx);
                EventBus.Fire(new CharacterSelectedEvent(a));
            };

            EventBus.Subscribe<PlacementModeChangedEvent>(e =>
            {
                Model.SetPlacementMode(e.IsActive);
                if (!e.IsActive) Model.SetSelectedIndex(-1);
            });
        }
    }
}
