// Assets/_Game/Scripts/Runtime/Combat/BaseHealthSystem.cs
using UnityEngine;
using _Game.Core.Events;
using _Game.Interfaces;
using _Game.Runtime.Characters;

namespace _Game.Runtime.Combat
{
    /// Tracks base HP; reacts when enemies reach bottom; despawns them via the normal death flow.
    public sealed class BaseHealthSystem : IUpdatableSystem
    {
        private readonly IEventBus _bus;
        private readonly CharacterRepository _repo;
        private bool _alreadyLost;

        public int MaxHp { get; }
        public int CurrentHp { get; private set; }

        public BaseHealthSystem(IEventBus bus, CharacterRepository repo, int maxHp = 10)
        {
            _bus = bus; _repo = repo;
            MaxHp = Mathf.Max(1, maxHp);
            CurrentHp = MaxHp;
            _bus.Subscribe<EnemyReachedBaseEvent>(OnEnemyReachedBase);
        }

        public void Tick() { }

        private void OnEnemyReachedBase(EnemyReachedBaseEvent e)
        {
            const int amount = 1;
            CurrentHp = Mathf.Max(0, CurrentHp - amount);
            _bus.Fire(new BaseDamagedEvent(amount, CurrentHp));
            Debug.Log($"[GameState] Base damaged (-{amount}). HP: {CurrentHp}/{MaxHp}");

            if (_repo.TryGetById(e.EnemyId, out var ent))
                _bus.Fire(new CharacterDiedEvent(ent)); // CharacterLifetimeSystem will clean up

            if (!_alreadyLost && CurrentHp <= 0)
            {
                _alreadyLost = true;
                Debug.Log("[GameState] LOSE — Base HP reached 0.");
                _bus.Fire(new GameLostEvent());
            }
        }
    }
}