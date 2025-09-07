// Assets/_Game/Scripts/Runtime/Combat/GameStateSystem.cs
using System.Collections.Generic;
using UnityEngine;
using _Game.Interfaces;
using _Game.Core.Events;
using _Game.Runtime.Levels;

namespace _Game.Runtime.Combat
{
    /// <summary>
    /// Win/Lose controller without repository dependency:
    /// - Tracks spawned enemy IDs via EnemySpawnedEvent.
    /// - Lose immediately on EnemyReachedBaseEvent (first hit ends the game).
    /// - Win when all planned enemies were spawned AND no live enemies remain
    ///   (i.e., every spawned enemy either died or reached the base).
    /// 
    /// This avoids needing CharacterRepository to filter roles:
    /// we only ever count IDs that were actually spawned as ENEMIES.
    /// </summary>
    public sealed class GameStateSystem : IUpdatableSystem
    {
        private readonly IEventBus _bus;
        private readonly LevelRuntimeConfig _level;

        private readonly int _totalPlannedEnemies;
        private int _spawnedEnemies;
        private bool _lost;
        private bool _won;

        // Track "live" enemies by their entity ids
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

        public void Tick() { /* event-driven */ }

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

#if UNITY_EDITOR
            // Debug.Log($"[GameState] Spawned { _spawnedEnemies }/{ _totalPlannedEnemies }, Live={ _live.Count }");
#endif
        }

        private void OnCharacterDied(CharacterDiedEvent e)
        {
            if (_won || _lost) return;

            // Count only if it is a tracked enemy (spawned by spawner)
            if (_live.Remove(e.Entity.EntityId))
            {
                TryWin();
            }
        }

        private void OnEnemyReachedBase(EnemyReachedBaseEvent e)
        {
            if (_won || _lost) return;

            // Remove from live if it was tracked, then lose immediately
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
