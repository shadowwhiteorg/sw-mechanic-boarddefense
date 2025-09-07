using _Game.Core.Events;
using _Game.Interfaces;

namespace _Game.Core.DI
{
    public sealed class GameInstaller : BaseInstaller
    {
        public override void Install(IDIContainer container)
        {
            var events = new EventBus();
            var systems = new SystemRunner();

            container.BindSingleton<IEventBus>(events);
            container.BindSingleton<ISystemRunner>(systems);

            GameContext.Events = events;
            GameContext.Systems = systems;
        }
    }
}