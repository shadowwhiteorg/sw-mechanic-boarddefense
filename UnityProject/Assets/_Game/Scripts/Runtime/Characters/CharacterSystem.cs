using System.Collections.Generic;
using _Game.Interfaces;
using UnityEngine;

namespace _Game.Runtime.Characters
{
    public sealed class CharacterSystem : IUpdatableSystem
    {
        private readonly List<ICharacterPlugin> _tickables = new();

        public void Register(CharacterEntity e)
        {
            foreach (var p in e.Plugins)
                _tickables.Add(p);
        }

        public void Unregister(CharacterEntity e)
        {
            foreach (var p in e.Plugins)
            {
                p.OnDespawn();
                _tickables.Remove(p);
            }
        }

        public void Tick()
        {
            var dt = Time.deltaTime;
            for (int i = 0; i < _tickables.Count; i++)
                _tickables[i].Tick(dt);
        }
    }
}