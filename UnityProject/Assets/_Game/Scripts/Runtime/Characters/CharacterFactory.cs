using _Game.Core;
using UnityEngine;
using _Game.Enums;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Characters.Config;
using _Game.Runtime.Characters.Plugins;
using _Game.Runtime.Characters.View;
using _Game.Runtime.Combat;
using _Game.Runtime.Core;

namespace _Game.Runtime.Characters
{
    public sealed class CharacterFactory
    {
        private int _nextId = 1;
        private readonly CharacterPoolRegistry _pools;

        public CharacterFactory(CharacterPoolRegistry pools)
        {
            _pools = pools;
        }
        public CharacterEntity Spawn(CharacterArchetype archetype, Cell cell, Transform parent, CharacterRole role)
        {
            return InternalCreate(archetype, role, parent);
        }
        
        public CharacterEntity SpawnAtWorld(
            CharacterArchetype archetype,
            Vector3 worldPosition,
            Cell cell,
            Transform parent,
            CharacterRole role)
        {
            var entity = InternalCreate(archetype, role, parent);

            var root = entity.View.Root;
            root.SetParent(parent, worldPositionStays: true);
            root.position = worldPosition;

            entity.Cell = cell;

            return entity;
        }

        private CharacterEntity InternalCreate(CharacterArchetype archetype, CharacterRole role, Transform parent)
        {
            GameObject go;
            if (archetype.viewPrefab != null)
            {
                go = Object.Instantiate(archetype.viewPrefab, parent, worldPositionStays: true);
            }
            else
            {
                go = new GameObject("CharacterView");
                go.transform.SetParent(parent, worldPositionStays: true);
            }

            var view = go.GetComponent<ICharacterView>();
            if (view == null)
                view = go.AddComponent<CharacterView>();

            var id = _nextId++;
            var entity = new CharacterEntity(id, archetype, view)
            {
                Role = role
            };

            view.Bind(entity);
            view.Show();

            AttachDefaultPlugins(entity);

            return entity;
        }

        private void AttachDefaultPlugins(CharacterEntity e)
        {
            // Health on everyone
            e.AddPlugin(new HealthPlugin(e.Archetype.baseHealth));

            if (e.Role == CharacterRole.Defense)
            {
                var targets = GameContext.Container.Resolve<TargetingService>();
                e.AddPlugin(new RangedAttackPlugin(
                    targets,
                    ratePerSec: e.Archetype.attackRate,
                    damage: Mathf.RoundToInt(e.Archetype.attackDamage),
                    rangeBlocks: e.Archetype.attackRangeBlocks,
                    direction: e.Archetype.attackDirection));
            }
            else if (e.Role == CharacterRole.Enemy)
            {
                // TODO:
                // If you want constant path drift for all enemies without per-wave paths,
                // MovementPlugin can be added here with a default straight-down vector or lane path.
                // Otherwise EnemySpawnerSystem adds a MovementPlugin using wave/path assets.
            }

            GameContext.Container.Resolve<CharacterSystem>().Register(e);
        }
    }
}
