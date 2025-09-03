using System;
using System.Collections.Generic;
using _Game.Interfaces;

namespace _Game.Core.Events
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new();

        public void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            _handlers[type] = _handlers.TryGetValue(type, out var existing)
                ? Delegate.Combine(existing, handler)
                : handler;
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                _handlers[type] = Delegate.Remove(existing, handler);
            }
        }

        public void Fire<T>(T eventData) where T : IGameEvent
        {
            if (_handlers.TryGetValue(typeof(T), out var handler))
            {
                (handler as Action<T>)?.Invoke(eventData);
            }
        }
    }
}