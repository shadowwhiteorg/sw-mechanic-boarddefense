using UnityEngine;
using _Game.Interfaces;
using _Game.Core.Events;

namespace _Game.Runtime.Combat
{
    public sealed class BaseHealthSystem : IUpdatableSystem
    {
        private readonly IEventBus _bus;

        public int MaxHp { get; }
        public int CurrentHp { get; private set; }

        private bool _lost;

        public BaseHealthSystem(IEventBus bus, int maxHp)
        {
            _bus = bus;
            MaxHp = Mathf.Max(1, maxHp);
            CurrentHp = MaxHp;

            _bus.Subscribe<EnemyReachedBaseEvent>(OnEnemyReachedBase);
        }

        public void Tick() {}

        private void OnEnemyReachedBase(EnemyReachedBaseEvent _)
        {
            if (_lost) return;

            CurrentHp = Mathf.Max(0, CurrentHp - 1);
            _bus.Fire(new BaseHealthChangedEvent(CurrentHp, MaxHp));

            if (CurrentHp <= 0 && !_lost)
            {
                _lost = true;
                Debug.Log("[GameState] Base HP reached 0 → LOST");
                _bus.Fire(new GameLostEvent());
            }
        }
    }
}