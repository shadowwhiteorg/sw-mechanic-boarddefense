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
    /// Spawns enemies per LevelRuntimeConfig.Waves.
    public sealed class EnemySpawnerSystem : IUpdatableSystem
    {
        private readonly BoardGrid _grid;
        private readonly GridProjector _projector;
        private readonly CharacterFactory _factory;
        private readonly CharacterRepository _repo;
        private readonly LevelRuntimeConfig _level;
        private readonly Transform _unitsParent;
        private readonly List<IEnumerator> _co = new();

        public EnemySpawnerSystem(BoardGrid grid, GridProjector projector, CharacterFactory factory,
                                  CharacterRepository repo, LevelRuntimeConfig level, Transform unitsParent)
        {
            _grid = grid; _projector = projector; _factory = factory; _repo = repo; _level = level; _unitsParent = unitsParent;

            // Start a coroutine per wave
            foreach (var w in _level.Waves)
                _co.Add(RunWave(w));
        }

        public void Tick()
        {
            for (int i = _co.Count - 1; i >= 0; i--)
                if (!_co[i].MoveNext()) _co.RemoveAt(i);
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

        private void SpawnOne(_Game.Runtime.Characters.Config.CharacterArchetype a)
        {
            int topRow = _grid.Size.Rows - 1;
            int col = Random.Range(0, _grid.Size.Cols);
            var entry = new Cell(topRow, col);
            var world = _projector.CellToWorldCenter(entry);

            var e = _factory.SpawnAtWorld(a, world, entry, _unitsParent, CharacterRole.Enemy);
            _repo.Add(e, entry); // track occupancy if you use it
        }
    }
}
