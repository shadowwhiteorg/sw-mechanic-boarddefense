using System;

namespace _Game.Interfaces
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler) where T : IGameEvent;
        void Unsubscribe<T>(Action<T> handler) where T : IGameEvent;
        void Fire<T>(T eventData) where T : IGameEvent;
    }
}