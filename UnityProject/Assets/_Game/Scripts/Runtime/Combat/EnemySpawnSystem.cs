using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Game.Interfaces;
using _Game.Enums;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Characters.Plugins;
using _Game.Runtime.Levels;

namespace _Game.Runtime.Combat
{
    public sealed class EnemySpawnerSystem : IUpdatableSystem
    {
        private readonly BoardGrid _grid;
        private readonly CharacterFactory _factory;
        private readonly CharacterRepository _repo;
        private readonly LevelRuntimeConfig _level;
        private readonly Transform _unitsParent;

        private readonly List<IEnumerator> _running = new();

        public EnemySpawnerSystem(
            BoardGrid grid,
            CharacterFactory factory,
            CharacterRepository repo,
            LevelRuntimeConfig level,
            Transform unitsParent)
        {
            _grid = grid; _factory = factory; _repo = repo; _level = level; _unitsParent = unitsParent;

            foreach (var w in _level.Waves)
                _running.Add(RunWave(w));
        }

        public void Tick()
        {
            for (int i = _running.Count - 1; i >= 0; i--)
            {
                if (!_running[i].MoveNext())
                    _running.RemoveAt(i);
            }
        }

        private IEnumerator RunWave(EnemyWave wave)
        {
            float t = 0f;
            while (t < wave.delayBefore) { t += Time.deltaTime; yield return null; }

            int spawned = 0;
            float cd = 0f;
            var path = (_level.Paths != null && wave.pathIndex >= 0 && wave.pathIndex < _level.Paths.Count)
                ? _level.Paths[wave.pathIndex] : null;

            while (spawned < wave.count)
            {
                if (cd <= 0f)
                {
                    SpawnOne(wave.enemyArchetype, path);
                    spawned++;
                    cd = Mathf.Max(0.05f, wave.spawnInterval);
                }
                cd -= Time.deltaTime;
                yield return null;
            }
        }

        private void SpawnOne(_Game.Runtime.Characters.Config.CharacterArchetype a, PathAsset path)
        {
            // Pick a random top-row column for entry
            int col = Random.Range(0, _grid.Size.Cols);
            var entry = new _Game.Runtime.Core.Cell(_grid.Size.Rows - 1, col); // top edge
            var world = Vector3.zero; // you may compute a spawn world pos ahead of first waypoint

            var e = _factory.SpawnAtWorld(a, world, entry, _unitsParent, CharacterRole.Enemy);
            _repo.Add(e, entry);

            // Attach path movement (factory also attaches by default; we add/override if path exists)
            if (path != null && path.waypoints != null && path.waypoints.Length > 0)
            {
                var wp = new Vector3[path.waypoints.Length];
                for (int i = 0; i < wp.Length; i++) wp[i] = path.waypoints[i].position;

                e.AddPlugin(new MovementPlugin(a.moveSpeed, wp));
            }
        }
    }
}
