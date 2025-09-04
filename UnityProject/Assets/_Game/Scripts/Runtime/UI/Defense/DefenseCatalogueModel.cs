using System.Collections.Generic;
using _Game.Runtime.Characters.Config;

namespace _Game.Systems.UI.Defense
{
    public sealed class DefenseCatalogueModel : BaseUIModel
    {
        public IReadOnlyList<CharacterArchetype> Items { get; private set; }
        public int SelectedIndex { get; private set; } = -1;
        public bool PlacementModeActive { get; private set; }

        public void SetItems(IReadOnlyList<CharacterArchetype> items)
        {
            Items = items;
            SelectedIndex = -1;
            NotifyUpdated();
        }

        public void SetSelectedIndex(int index)
        {
            if (SelectedIndex == index) return;
            SelectedIndex = index;
            NotifyUpdated();
        }

        public void SetPlacementMode(bool active)
        {
            if (PlacementModeActive == active) return;
            PlacementModeActive = active;
            NotifyUpdated();
        }
    }
}