using UnityEngine;
using _Game.Core.Events;
using _Game.Interfaces;
using _Game.Runtime.Characters;

namespace _Game.Runtime.Combat
{
    public sealed class BaseHealthSystem : IUpdatableSystem
    {
        private readonly IEventBus _bus;
        private readonly CharacterRepository _repo;

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

            if (_repo.TryGetById(e.EnemyId, out var ent))
                _bus.Fire(new CharacterDiedEvent(ent)); // CharacterLifetimeSystem will clean up

            // Optionally: if (CurrentHp <= 0) _bus.Fire(new GameLostEvent());
        }
    }
}