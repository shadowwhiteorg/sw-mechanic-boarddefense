using UnityEngine;
using _Game.Core.DI;
using _Game.Interfaces;
using _Game.Core;  

namespace _Game.Systems.UI.Defense
{
    public sealed class UIInstaller : BaseInstaller
    {
        [Header("Defense Catalogue")]
        [SerializeField] private DefenseCatalogueView   defenseCatalogueView;
        [SerializeField] private DefenseCatalogueScreen defenseCatalogueScreen;

        public override void Install(IDIContainer container)
        {
            IEventBus events = GameContext.Events;

            if (!defenseCatalogueView)
                defenseCatalogueView = FindFirstObjectByType<DefenseCatalogueView>(FindObjectsInactive.Include);
            if (!defenseCatalogueScreen)
                defenseCatalogueScreen = FindFirstObjectByType<DefenseCatalogueScreen>(FindObjectsInactive.Include);

            if (!defenseCatalogueView || !defenseCatalogueScreen)
            {
                Debug.LogWarning("[UIInstaller] Defense Catalogue View/Screen not found in scene. Skipping.");
                return;
            }

            var defModel = new DefenseCatalogueModel();

            container.BindSingleton(defModel);

            defenseCatalogueScreen.Construct(defModel, defenseCatalogueView, events);

        }
    }
}
