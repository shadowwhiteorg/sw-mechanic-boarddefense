using UnityEngine;
using _Game.Core.Events;
using _Game.Interfaces;
using _Game.Runtime.Characters;

namespace _Game.Runtime.Combat
{
    public sealed class CharacterLifetimeSystem : IUpdatableSystem
    {
        private readonly CharacterRepository _repo;
        private readonly IEventBus _events;

        public CharacterLifetimeSystem(CharacterRepository repo, IEventBus events)
        {
            _repo = repo; _events = events;
            _events.Subscribe<CharacterDiedEvent>(OnDied);
        }

        public void Tick() { /* no-op */ }

        private void OnDied(CharacterDiedEvent e)
        {
            var ent = e.Entity;
            if (ent?.View?.Root)
                Object.Destroy(ent?.View.Root.gameObject);

            _repo.Remove(ent);
            _events.Fire(new CharacterDespawnedEvent(ent.EntityId));
        }
    }
}