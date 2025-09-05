using System.Collections.Generic;
using _Game.Interfaces;

namespace _Game.Runtime.Characters
{
    public sealed class CharacterSystem : IUpdatableSystem
    {
        private readonly List<ICharacterPlugin> _tickables = new();

        public void Register(CharacterEntity e)
        {
            for (int i = 0; i < e.Plugins.Count; i++)
            {
                var p = e.Plugins[i];
                p.OnSpawn(e);
                _tickables.Add(p);
            }
        }

        public void Unregister(CharacterEntity e)
        {
            for (int i = 0; i < e.Plugins.Count; i++)
            {
                var p = e.Plugins[i];
                p.OnDespawn();
                _tickables.Remove(p);
            }
        }

        public void Tick()
        {
            float dt = UnityEngine.Time.deltaTime;
            for (int i = 0; i < _tickables.Count; i++)
                _tickables[i].Tick(dt);
        }
    }
}