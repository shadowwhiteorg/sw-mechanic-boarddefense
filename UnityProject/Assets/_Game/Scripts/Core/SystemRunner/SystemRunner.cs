using System.Collections.Generic;
using _Game.Interfaces;

namespace _Game.Core
{
    public class SystemRunner : ISystemRunner
    {
        private readonly List<IUpdatableSystem> _updateSystems = new();
        private readonly List<IFixedUpdatableSystem> _fixedUpdateSystems = new();

        public void Register(IUpdatableSystem system)
        {
            if (!_updateSystems.Contains(system))
                _updateSystems.Add(system);
        }

        public void Register(IFixedUpdatableSystem system)
        {
            if (!_fixedUpdateSystems.Contains(system))
                _fixedUpdateSystems.Add(system);
        }

        public void Unregister(IUpdatableSystem system) => _updateSystems.Remove(system);
        public void Unregister(IFixedUpdatableSystem system) => _fixedUpdateSystems.Remove(system);

        public void Tick()
        {
            foreach (var system in _updateSystems)
                system.Tick();
        }

        public void FixedTick()
        {
            foreach (var system in _fixedUpdateSystems)
                system.FixedTick();
        }
    }
}