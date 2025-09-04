using _Game.Interfaces;

namespace _Game.Core
{
    public static class GameContext
    {
        public static IDIContainer Container { get; internal set; }
        public static IEventBus Events { get; internal set; }
        public static ISystemRunner Systems { get; internal set; }
    }
}