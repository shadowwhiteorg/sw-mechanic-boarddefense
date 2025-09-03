using System;
using System.Collections.Generic;
using _Game.Interfaces;

namespace _Game.Core.DI
{
    public class DIContainer : IDIContainer
    {
        private readonly Dictionary<Type, object> _singletons = new();
        private readonly Dictionary<Type, Type> _transients = new();

        public void BindSingleton<T>(T instance) => _singletons[typeof(T)] = instance;
        public void Bind<TInterface, TImplementation>() where TImplementation : TInterface => _transients[typeof(TInterface)] = typeof(TImplementation);

        public T Resolve<T>() {
            if (_singletons.TryGetValue(typeof(T), out var singleton)) 
                return (T)singleton;

            if (_transients.TryGetValue(typeof(T), out var implType)) 
                return (T)Activator.CreateInstance(implType);

            throw new Exception($"Color {typeof(T)} not registered.");
        }
    }
}

