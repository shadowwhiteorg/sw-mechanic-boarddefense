using UnityEngine;
using _Game.Interfaces;
using _Game.Core.Events;
using _Game.Runtime.Characters;

namespace _Game.Runtime.Combat
{
    public sealed class CharacterLifetimeSystem : IUpdatableSystem
    {
        private readonly CharacterRepository _repo;
        private readonly CharacterSystem _charSystem;
        private readonly IEventBus _bus;

        public CharacterLifetimeSystem(CharacterRepository repo, IEventBus bus)
        {
            _repo = repo;
            _bus = bus;
            _charSystem = _Game.Core.GameContext.Container.Resolve<CharacterSystem>();

            _bus.Subscribe<CharacterDiedEvent>(OnCharacterDied);
        }

        public void Tick() { /* no-op */ }

        private void OnCharacterDied(CharacterDiedEvent e)
        {
            var ent = e.Entity;
            if (ent == null) return;

            _charSystem.Unregister(ent);

            _repo.Remove(ent);

            var root = ent.View != null ? ent.View.Root : null;
            if (root != null) Object.Destroy(root.gameObject);
        }
    }
}