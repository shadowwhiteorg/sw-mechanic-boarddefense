// Assets/_Game/Scripts/Runtime/Core/EnemySpawnerSystem.cs
using System.Collections.Generic;
using UnityEngine;
using _Game.Interfaces;
using _Game.Enums;
using _Game.Core.Events;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Characters.Config;
using _Game.Runtime.Levels;

namespace _Game.Runtime.Core
{
    /// <summary>
    /// Spawns enemies based on LevelRuntimeConfig enemy counts (per archetype).
    /// Each spawn appears at TOP row, RANDOM column. Stops when counts are exhausted.
    /// Fires EnemySpawnedEvent(entityId) after each spawn.
    /// </summary>
    public sealed class EnemySpawner : IUpdatableSystem
    {
        private readonly BoardGrid _grid;
        private readonly GridProjector _projector;
        private readonly CharacterFactory _factory;
        private readonly LevelRuntimeConfig _level;
        private readonly Transform _unitsParent;
        private readonly IEventBus _bus;

        private readonly Dictionary<CharacterArchetype, int> _remaining = new();
        private readonly List<CharacterArchetype> _candidates = new();

        private readonly float _spawnInterval;
        private readonly float _startDelay;

        private float _cd;
        private float _delay;
        private bool _done;

        /// <param name="spawnInterval">Seconds between spawns.</param>
        /// <param name="startDelay">Initial delay before first spawn.</param>
        public EnemySpawner(
            BoardGrid grid,
            GridProjector projector,
            CharacterFactory factory,
            LevelRuntimeConfig level,
            Transform unitsParent,
            IEventBus bus,
            float spawnInterval = 1.0f,
            float startDelay = 0.0f)
        {
            _grid        = grid;
            _projector   = projector;
            _factory     = factory;
            _level       = level;
            _unitsParent = unitsParent;
            _bus         = bus;

            _spawnInterval = Mathf.Max(0.05f, spawnInterval);
            _startDelay    = Mathf.Max(0f, startDelay);
            _delay         = _startDelay;

            // Snapshot counts from the level (read-only at runtime)
            if (_level?.EnemyRemaining != null)
            {
                foreach (var kv in _level.EnemyRemaining)
                {
                    if (kv.Key != null && kv.Value > 0)
                    {
                        _remaining[kv.Key] = kv.Value;
                        _candidates.Add(kv.Key);
                    }
                }
            }
        }

        public void Tick()
        {
            if (_done) return;

            if (_delay > 0f)
            {
                _delay -= Time.deltaTime;
                return;
            }

            _cd -= Time.deltaTime;
            if (_cd > 0f) return;

            if (!TryPick(out var arch))
            {
                _done = true;
                return;
            }

            SpawnOne(arch);
            _remaining[arch] -= 1;
            if (_remaining[arch] <= 0)
            {
                _candidates.Remove(arch);
            }

            _cd = _spawnInterval;
        }

        private bool TryPick(out CharacterArchetype a)
        {
            a = null;
            if (_candidates.Count == 0) return false;

            // Randomly pick a candidate that still has stock
            for (int safety = 0; safety < 8 && _candidates.Count > 0; safety++)
            {
                int idx = Random.Range(0, _candidates.Count);
                var cand = _candidates[idx];
                if (_remaining.TryGetValue(cand, out var n) && n > 0)
                {
                    a = cand;
                    return true;
                }
                _candidates.RemoveAt(idx);
            }
            return a != null;
        }

        private void SpawnOne(CharacterArchetype arch)
        {
            int topRow = _grid.Size.Rows - 1;
            int col    = Random.Range(0, _grid.Size.Cols);
            var cell   = new Cell(topRow, col);

            var world = _projector.CellToWorldCenter(cell);

            var entity = _factory.SpawnAtWorld(
                arch,
                world,
                cell,
                _unitsParent,
                CharacterRole.Enemy
            );

            // Announce the spawn so GameStateSystem can track live enemies without repo
            _bus.Fire(new EnemySpawnedEvent(entity.EntityId));
        }
    }
}
