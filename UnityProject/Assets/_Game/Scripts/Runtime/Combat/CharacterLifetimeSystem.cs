// Assets/_Game/Scripts/Runtime/Combat/CharacterLifetimeSystem.cs
using UnityEngine;
using _Game.Interfaces;
using _Game.Core.Events;
using _Game.Runtime.Characters;

namespace _Game.Runtime.Combat
{
    /// Handles CharacterDiedEvent: unregisters, removes from repo, destroys view.
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

            // 1) Unregister from character ticking (stops plugins immediately)
            _charSystem.Unregister(ent);

            // 2) Remove from repository mappings
            _repo.Remove(ent);

            // 3) Destroy the view (safe-guard “destroyed but accessed” issues)
            var root = ent.View != null ? ent.View.Root : null;
            if (root != null) Object.Destroy(root.gameObject);
        }
    }
}