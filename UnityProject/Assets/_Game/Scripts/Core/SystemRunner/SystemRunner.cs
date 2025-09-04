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
            if (system != null && !_updateSystems.Contains(system))
                _updateSystems.Add(system);
        }

        public void Register(IFixedUpdatableSystem system)
        {
            if (system != null && !_fixedUpdateSystems.Contains(system))
                _fixedUpdateSystems.Add(system);
        }

        public void Tick()
        {
            for (int i = 0; i < _updateSystems.Count; i++)
                _updateSystems[i].Tick();
        }

        public void FixedTick()
        {
            for (int i = 0; i < _fixedUpdateSystems.Count; i++)
                _fixedUpdateSystems[i].FixedTick();
        }
    }
}