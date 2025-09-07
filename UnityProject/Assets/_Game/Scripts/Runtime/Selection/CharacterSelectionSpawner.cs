// Assets/_Game/Scripts/Runtime/Selection/CharacterSelectionSpawner.cs
using System.Collections.Generic;
using UnityEngine;
using _Game.Runtime.Levels;
using _Game.Runtime.Characters.Config;

namespace _Game.Runtime.Selection
{
    /// <summary>
    /// Spawns exactly one selectable per defense archetype defined in the current level,
    /// evenly spaced between two border points. Also exposes SpawnAt() for single refills.
    /// 
    /// Notes:
    /// - We intentionally do NOT decrement level counts here. Counts represent "extra copies"
    ///   available after the first placement; the selection system handles decrement on refill.
    /// </summary>
    public sealed class CharacterSelectionSpawner
    {
        private readonly LevelRuntimeConfig _level;
        private readonly Transform _left;
        private readonly Transform _right;
        private readonly Transform _parent;

        public CharacterSelectionSpawner(LevelRuntimeConfig level,
                                         Transform spawnBorderLeft,
                                         Transform spawnBorderRight,
                                         Transform parent)
        {
            _level  = level;
            _left   = spawnBorderLeft;
            _right  = spawnBorderRight;
            _parent = parent;
        }

        /// <summary>Initial lineup: one selectable per defense archetype listed by the level.</summary>
        public List<SelectableCharacterView> Spawn()
        {
            var result = new List<SelectableCharacterView>();
            if (_left == null || _right == null) return result;

            // Prefer LevelData.defenses (explicit design list). Fallback to runtime dictionary keys.
            var arches = new List<CharacterArchetype>();
            if (_level?.Source != null && _level.Source.defenses != null && _level.Source.defenses.Count > 0)
            {
                foreach (var e in _level.Source.defenses)
                    if (e.archetype) arches.Add(e.archetype);
            }
            else if (_level?.DefenseRemaining != null)
            {
                foreach (var kv in _level.DefenseRemaining)
                    if (kv.Key) arches.Add(kv.Key);
            }

            int n = arches.Count;
            if (n <= 0) return result;

            Vector3 A = _left.position;
            Vector3 B = _right.position;

            for (int i = 0; i < n; i++)
            {
                float t = (n == 1) ? 0.5f : (float)i / (n - 1);
                Vector3 anchor = Vector3.Lerp(A, B, t);

                var view = SpawnAt(arches[i], anchor);
                if (view) result.Add(view);
            }

            return result;
        }

        /// <summary>Spawn a single selectable for the given archetype at the exact world position.</summary>
        public SelectableCharacterView SpawnAt(CharacterArchetype archetype, Vector3 worldPos)
        {
            if (!archetype || !archetype.viewPrefab) return null;

            var go = Object.Instantiate(archetype.viewPrefab, _parent, true);
            go.name = $"Selectable_{(string.IsNullOrWhiteSpace(archetype.displayName) ? archetype.name : archetype.displayName)}";
            go.transform.position = worldPos;

            var sel = go.GetComponent<SelectableCharacterView>();
            if (!sel)
            {
                Object.Destroy(go);
                Debug.LogWarning($"[SelectionSpawner] Prefab for '{archetype?.displayName ?? archetype?.name}' lacks SelectableCharacterView.");
                return null;
            }

            sel.Initialize(archetype);
            sel.SetAsSelectable(true);
            return sel;
        }
    }
}
