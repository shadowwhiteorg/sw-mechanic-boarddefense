using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Game.Interfaces;
using _Game.Enums;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Levels;
using _Game.Runtime.Core;

namespace _Game.Runtime.Combat
{
    /// <summary>
    /// Spawns enemies using LevelRuntimeConfig.Waves.
    /// Each enemy starts at TOP row in a RANDOM column and gets lane movement via the factory.
    /// </summary>
    public sealed class EnemySpawnerSystem : IUpdatableSystem
    {
        private readonly BoardGrid _grid;
        private readonly GridProjector _projector;
        private readonly CharacterFactory _factory;
        private readonly CharacterRepository _repo;
        private readonly LevelRuntimeConfig _level;
        private readonly Transform _unitsParent;

        private readonly List<IEnumerator> _running = new();

        public EnemySpawnerSystem(
            BoardGrid grid,
            GridProjector projector,
            CharacterFactory factory,
            CharacterRepository repo,
            LevelRuntimeConfig level,
            Transform unitsParent)
        {
            _grid = grid;
            _projector = projector;
            _factory = factory;
            _repo = repo;
            _level = level;
            _unitsParent = unitsParent;

            // Start a small coroutine per wave
            foreach (var w in _level.Waves)
                _running.Add(RunWave(w));
        }

        public void Tick()
        {
            for (int i = _running.Count - 1; i >= 0; i--)
                if (!_running[i].MoveNext())
                    _running.RemoveAt(i);
        }

        private IEnumerator RunWave(EnemyWave wave)
        {
            float t = 0f;
            while (t < wave.delayBefore) { t += Time.deltaTime; yield return null; }

            int spawned = 0;
            float cd = 0f;

            while (spawned < wave.count)
            {
                if (cd <= 0f)
                {
                    SpawnOne(wave.enemyArchetype);
                    spawned++;
                    cd = Mathf.Max(0.05f, wave.spawnInterval);
                }
                cd -= Time.deltaTime;
                yield return null;
            }
        }

        private void SpawnOne(_Game.Runtime.Characters.Config.CharacterArchetype archetype)
        {
            int topRow = _grid.Size.Rows - 1;
            int col = Random.Range(0, _grid.Size.Cols);
            var cell = new Cell(topRow, col);

            var world = _projector.CellToWorldCenter(cell);

            var e = _factory.SpawnAtWorld(
                archetype,
                world,
                cell,
                _unitsParent,
                CharacterRole.Enemy
            );

            // Track in repository for occupancy/targeting
            _repo.Add(e, cell);
        }
    }
}
