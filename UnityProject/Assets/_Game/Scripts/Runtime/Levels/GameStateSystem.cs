using System.Collections.Generic;
using UnityEngine;
using _Game.Interfaces;
using _Game.Core.Events;
using _Game.Runtime.Levels;

namespace _Game.Runtime.Combat
{
   
    public sealed class GameStateSystem : IUpdatableSystem
    {
        private readonly IEventBus _bus;
        private readonly LevelRuntimeConfig _level;

        private readonly int _totalPlannedEnemies;
        private int _spawnedEnemies;
        private bool _lost;
        private bool _won;

        private readonly HashSet<int> _live = new();

        public GameStateSystem(IEventBus bus, LevelRuntimeConfig level)
        {
            _bus   = bus;
            _level = level;

            _totalPlannedEnemies = SumPlannedEnemies(level);

            _bus.Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            _bus.Subscribe<CharacterDiedEvent>(OnCharacterDied);
            _bus.Subscribe<EnemyReachedBaseEvent>(OnEnemyReachedBase);

#if UNITY_EDITOR
            Debug.Log($"[GameState] Planned enemies: #{_totalPlannedEnemies}");
#endif
        }

        public void Tick() {  }

        private static int SumPlannedEnemies(LevelRuntimeConfig level)
        {
            int sum = 0;
            if (level?.EnemyRemaining != null)
            {
                foreach (var kv in level.EnemyRemaining)
                    sum += Mathf.Max(0, kv.Value);
            }
            return sum;
        }

        private void OnEnemySpawned(EnemySpawnedEvent e)
        {
            if (_won || _lost) return;

            _spawnedEnemies++;
            _live.Add(e.EntityId);
            
        }

        private void OnCharacterDied(CharacterDiedEvent e)
        {
            if (_won || _lost) return;

            if (_live.Remove(e.Entity.EntityId))
            {
                TryWin();
            }
        }

        private void OnEnemyReachedBase(EnemyReachedBaseEvent e)
        {
            if (_won || _lost) return;

            _live.Remove(e.EnemyId);

            _lost = true;
            Debug.Log("[GameState] Enemy reached base → LOST");
            _bus.Fire(new GameLostEvent());
        }

        private void TryWin()
        {
            if (_won || _lost) return;

            if (_spawnedEnemies >= _totalPlannedEnemies && _live.Count == 0)
            {
                _won = true;
                Debug.Log("[GameState] All planned enemies resolved → WON");
                _bus.Fire(new GameWonEvent());
            }
        }
    }
}
