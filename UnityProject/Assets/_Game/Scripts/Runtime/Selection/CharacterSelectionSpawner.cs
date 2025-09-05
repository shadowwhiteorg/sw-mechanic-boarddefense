using System.Collections.Generic;
using UnityEngine;
using _Game.Runtime.Levels;
using _Game.Runtime.Selection;
using _Game.Runtime.Characters.Config;

namespace _Game.Runtime.Selection
{
    /// <summary>
    /// Spawns selectable defense characters in 3D space at game start.
    /// No physics: these are just visual models the player can drag directly.
    /// </summary>
    public sealed class CharacterSelectionSpawner
    {
        private readonly LevelRuntimeConfig _levelConfig;
        private readonly Transform _spawnPoint;
        private readonly float _spacing;
        private readonly Transform _parent;

        /// <param name="config">Runtime level config with AllowedDefenseArchetypes.</param>
        /// <param name="spawnPoint">Anchor point for the first selectable.</param>
        /// <param name="spacing">World-units gap between spawned models.</param>
        /// <param name="parent">Parent transform for keeping hierarchy tidy.</param>
        public CharacterSelectionSpawner(LevelRuntimeConfig config, Transform spawnPoint, float spacing, Transform parent)
        {
            _levelConfig = config;
            _spawnPoint  = spawnPoint;
            _spacing     = Mathf.Max(0.01f, spacing);
            _parent      = parent;
        }

        /// <summary>Instantiates one selectable per allowed archetype and returns the list.</summary>
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
