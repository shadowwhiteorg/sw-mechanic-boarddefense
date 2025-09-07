// Assets/_Game/Scripts/Runtime/Selection/SelectionHudController.cs
using System.Collections.Generic;
using UnityEngine;
using _Game.Core.Events;
using _Game.Interfaces;
using _Game.Runtime.Levels;
using _Game.Runtime.Characters.Config;

namespace _Game.Runtime.Selection
{
    /// <summary>
    /// Spawns one SelectionSlotHud per archetype slot and keeps it updated.
    /// Now reads evenly-spaced slot anchors directly from CharacterSelectionSpawner.
    /// </summary>
    public sealed class SelectionHudController
    {
        private readonly LevelRuntimeConfig _level;
        private readonly IEventBus _events;
        private readonly Camera _camera;
        private readonly GameObject _hudPrefab;
        private readonly Transform _hudParent;
        private readonly float _yOffset;

        private readonly Dictionary<CharacterArchetype, SelectionSlotHud> _hudByArch = new();

        public SelectionHudController(LevelRuntimeConfig level,
                                      IEventBus events,
                                      Camera camera,
                                      GameObject hudPrefab,
                                      Transform hudParent,
                                      float yOffsetWorld)
        {
            _level = level;
            _events = events;
            _camera = camera;
            _hudPrefab = hudPrefab;
            _hudParent = hudParent;
            _yOffset = yOffsetWorld;

            _events.Subscribe<CharacterPlacedEvent>(OnPlaced);
        }

        /// <summary>
        /// Create one HUD per slot using the spawner’s evenly-spaced anchors.
        /// </summary>
        public void BuildFromSpawner(CharacterSelectionSpawner spawner)
        {
            if (spawner == null || _hudPrefab == null) return;

            foreach (var kv in spawner.SlotAnchors) // (archetype -> anchor)
            {
                RegisterSlot(kv.Key, kv.Value);
            }

            RefreshAll();
        }

        private void RegisterSlot(CharacterArchetype arch, Transform anchor)
        {
            if (!arch || anchor == null || _hudByArch.ContainsKey(arch)) return;

            var go = Object.Instantiate(_hudPrefab, _hudParent, worldPositionStays: true);
            go.name = $"HUD_{(string.IsNullOrWhiteSpace(arch.displayName) ? arch.name : arch.displayName)}";

            // place immediately at the anchor + Y offset
            go.transform.position = anchor.position + Vector3.up * _yOffset;

            var hud = go.GetComponent<SelectionSlotHud>();
            if (!hud)
            {
                Object.Destroy(go);
                Debug.LogWarning("[SelectionHudController] HUD prefab has no SelectionSlotHud component.");
                return;
            }

            hud.Initialize(arch, _level, anchor, _yOffset, _camera);
            _hudByArch[arch] = hud;
        }

        public void RefreshAll()
        {
            foreach (var kv in _hudByArch) kv.Value.RefreshAll();
        }

        private void OnPlaced(CharacterPlacedEvent e)
        {
            if (_hudByArch.TryGetValue(e.Archetype, out var hud))
                hud.RefreshAll();
        }
    }
}
