using UnityEngine;
using _Game.Enums;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Characters.Config;
using _Game.Runtime.Characters.Plugins;
using _Game.Runtime.Characters.View;
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

        /// <summary>
        /// Legacy spawn (position must be set by caller later). Prefer SpawnAtWorld().
        /// </summary>
        public CharacterEntity Spawn(CharacterArchetype archetype, Cell cell, Transform parent, CharacterRole role)
        {
            return InternalCreate(archetype, role, parent);
        }

        /// <summary>
        /// Correct, robust spawn: instantiates the view and constructs CharacterEntity
        /// with (id, archetype, view). Sets world position to the provided point.
        /// </summary>
        public CharacterEntity SpawnAtWorld(
            CharacterArchetype archetype,
            Vector3 worldPosition,
            Cell cell,
            Transform parent,
            CharacterRole role)
        {
            var entity = InternalCreate(archetype, role, parent);

            // Make sure we keep world space when parenting, then set world position.
            var root = entity.View.Root;
            root.SetParent(parent, worldPositionStays: true);
            root.position = worldPosition;

            // If you want a canonical rotation relative to the board, set it here.
            // root.rotation = Quaternion.identity;

            // Optionally store the cell on the entity if your model has it:
            entity.Cell = cell;

            return entity;
        }

        private CharacterEntity InternalCreate(CharacterArchetype archetype, CharacterRole role, Transform parent)
        {
            // Acquire/instantiate a view
            GameObject go;
            if (archetype.viewPrefab != null)
            {
                // Instantiate under parent, keep world transform (we'll set position later)
                go = Object.Instantiate(archetype.viewPrefab, parent, worldPositionStays: true);
            }
            else
            {
                go = new GameObject("CharacterView");
                go.transform.SetParent(parent, worldPositionStays: true);
            }

            // Ensure we have a view adapter
            var view = go.GetComponent<ICharacterView>();
            if (view == null)
                view = go.AddComponent<CharacterView>();

            // IMPORTANT: construct with the 3-arg ctor (id, archetype, view)
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

        private static void AttachDefaultPlugins(CharacterEntity e)
        {
            // Example default wiring; align with your actual plugin policy
            // TODO: Add plugins
            
            // e.AddPlugin(new HealthPlugin(e, max: e.Archetype.baseHealth));
            // if (e.Role == CharacterRole.Defense)
            // {
            //     e.AddPlugin(new MeleeAttackPlugin( e.Archetype.attackRate,e.Archetype.attackDamage));
            // }
            // else if (e.Role == CharacterRole.Enemy)
            // {
            //     e.AddPlugin(new MovementPlugin(e, e.Archetype.moveSpeed));
            // }
        }
    }
}
