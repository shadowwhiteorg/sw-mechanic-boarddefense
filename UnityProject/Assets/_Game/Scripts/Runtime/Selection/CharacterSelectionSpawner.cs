using System.Collections.Generic;
using _Game.Core.Events;
using _Game.Interfaces;
using UnityEngine;
using _Game.Runtime.Levels;

namespace _Game.Runtime.Selection
{
    public class CharacterSelectionSpawner
    {
        private readonly LevelRuntimeConfig _levelConfig;
        private readonly Transform _spawnPoint;
        private readonly float _spacing;
        private readonly IEventBus _eventBus;

        public CharacterSelectionSpawner(LevelRuntimeConfig config, Transform spawnPoint, float spacing, IEventBus eventBus)
        {
            _levelConfig = config;
            _spawnPoint = spawnPoint;
            _spacing = spacing;
            _eventBus = eventBus;
        }

        public List<SelectableCharacterView> Spawn()
        {
            List<SelectableCharacterView> list = new();
            float offset = 0f;

            foreach (var archetype in _levelConfig.AllowedDefenseArchetypes)
            {
                var go = Object.Instantiate(archetype.viewPrefab, _spawnPoint.position + new Vector3(offset, 0f, 0f), Quaternion.identity);
                go.name = $"Selectable_{archetype.displayName}";

                var view = go.GetComponent<SelectableCharacterView>();
                view.Initialize(archetype);
                if (view == null)
                {
                    Debug.LogWarning($"Missing SelectableCharacterView on prefab {go.name}");
                    continue;
                }

                view.OnClicked += (v) => _eventBus.Fire(new CharacterSelectedEvent(v.Archetype));
                list.Add(view);

                offset += _spacing;
            }

            return list;
        }
    }
}