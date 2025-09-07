// Assets/_Game/Scripts/Runtime/Selection/CharacterSelectionSpawner.cs
using System.Collections.Generic;
using UnityEngine;
using _Game.Runtime.Levels;
using _Game.Runtime.Characters.Config;

namespace _Game.Runtime.Selection
{
    /// <summary>
    /// Spawns exactly one selectable per defense archetype defined in the level,
    /// evenly spaced between two border points. Also exposes per-archetype slot anchors.
    /// </summary>
    public sealed class CharacterSelectionSpawner
    {
        private readonly LevelRuntimeConfig _level;
        private readonly Transform _left;
        private readonly Transform _right;
        private readonly Transform _parent;

        private Transform _anchorsRoot;
        private readonly Dictionary<CharacterArchetype, Transform> _anchors = new();

        /// <summary>World anchors for each archetype slot.</summary>
        public IReadOnlyDictionary<CharacterArchetype, Transform> SlotAnchors => _anchors;

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

            EnsureAnchorsRoot();
            ClearAnchors(); // defensive, in case of re-install

            // Get archetypes in a deterministic order (prefer LevelData order)
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
                var arch = arches[i];

                float t = (n == 1) ? 0.5f : (float)i / (n - 1);
                Vector3 pos = Vector3.Lerp(A, B, t);

                // Create & store an anchor for this slot
                var anchor = new GameObject($"SlotAnchor_{(string.IsNullOrWhiteSpace(arch.displayName) ? arch.name : arch.displayName)}").transform;
                anchor.SetParent(_anchorsRoot, true);
                anchor.position = pos;
                _anchors[arch] = anchor;

                // Spawn the selectable at that anchor
                var view = SpawnAt(arch, pos);
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

        private void EnsureAnchorsRoot()
        {
            if (_anchorsRoot) return;
            _anchorsRoot = new GameObject("SelectionSlotAnchors").transform;
            _anchorsRoot.SetParent(_parent, true);
        }

        private void ClearAnchors()
        {
            _anchors.Clear();
            if (_anchorsRoot)
            {
                for (int i = _anchorsRoot.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(_anchorsRoot.GetChild(i).gameObject);
            }
        }
    }
}
