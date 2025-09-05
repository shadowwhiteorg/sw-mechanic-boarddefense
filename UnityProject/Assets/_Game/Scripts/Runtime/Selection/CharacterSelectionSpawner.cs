using System.Collections.Generic;
using UnityEngine;
using _Game.Runtime.Levels;

namespace _Game.Runtime.Selection
{

    public sealed class CharacterSelectionSpawner
    {
        private readonly LevelRuntimeConfig _levelConfig;
        private readonly Transform _spawnPoint;
        private readonly float _spacing;
        private readonly Transform _parent;

        public CharacterSelectionSpawner(LevelRuntimeConfig config, Transform spawnPoint, float spacing, Transform parent)
        {
            _levelConfig = config;
            _spawnPoint  = spawnPoint;
            _spacing     = Mathf.Max(0.01f, spacing);
            _parent      = parent;
        }

        public List<SelectableCharacterView> Spawn()
        {
            var list   = new List<SelectableCharacterView>(_levelConfig.AllowedDefenseArchetypes.Count);
            float step = 0f;

            foreach (var archetype in _levelConfig.AllowedDefenseArchetypes)
            {
                if (archetype == null || archetype.viewPrefab == null)
                {
                    Debug.LogWarning("[CharacterSelectionSpawner] Missing archetype or viewPrefab; skipping.");
                    continue;
                }

                var pos = _spawnPoint.position + new Vector3(step, 0f, 0f);
                var go  = Object.Instantiate(archetype.viewPrefab, pos, Quaternion.identity, _parent);
                go.name = $"Selectable_{(string.IsNullOrWhiteSpace(archetype.displayName) ? archetype.name : archetype.displayName)}";

                var view = go.GetComponent<SelectableCharacterView>();
                if (view == null)
                {
                    Debug.LogWarning($"[CharacterSelectionSpawner] SelectableCharacterView missing on {go.name}; skipping.");
                    Object.Destroy(go);
                    continue;
                }

                view.Initialize(archetype);
                list.Add(view);
                step += _spacing;
            }

            return list;
        }
    }
}
