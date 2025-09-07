using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Interfaces;

namespace _Game.Core.DI
{
    public class DIContainer : IDIContainer
    {
        private readonly Dictionary<Type, object> _singletons = new();
        private readonly Dictionary<Type, Type> _transients = new();

        public void BindSingleton<T>(T instance) => _singletons[typeof(T)] = instance;

        public void Bind<TInterface, TImplementation>() where TImplementation : TInterface
            => _transients[typeof(TInterface)] = typeof(TImplementation);

        public T Resolve<T>() => (T)Resolve(typeof(T), new HashSet<Type>());

        private object Resolve(Type type, HashSet<Type> resolving)
        {
            if (_singletons.TryGetValue(type, out var singleton))
                return singleton;

            if (_transients.TryGetValue(type, out var implType))
                return CreateWithInjection(implType, resolving);

            if (!type.IsAbstract && !type.IsInterface)
                return CreateWithInjection(type, resolving);

            throw new Exception($"Type {type} is not registered in the DI container.");
        }

        private object CreateWithInjection(Type implType, HashSet<Type> resolving)
        {
            if (!resolving.Add(implType))
                throw new InvalidOperationException($"Circular dependency detected when resolving {implType}.");

            try
            {
                var ctors = implType.GetConstructors();
                if (ctors.Length == 0)
                    return Activator.CreateInstance(implType);

                var bestCtor = ctors
                    .OrderByDescending(c => c.GetParameters().Length)
                    .First();

                var parameters = bestCtor.GetParameters();
                if (parameters.Length == 0)
                    return Activator.CreateInstance(implType);

                var args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                    args[i] = Resolve(parameters[i].ParameterType, resolving);

                return bestCtor.Invoke(args);
            }
            finally
            {
                resolving.Remove(implType);
            }
        }
    }
}

