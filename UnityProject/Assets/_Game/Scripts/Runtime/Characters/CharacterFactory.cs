// Assets/_Game/Scripts/Runtime/Characters/CharacterFactory.cs
using UnityEngine;

using _Game.Core;                         // GameContext (DI access)
using _Game.Enums;
using _Game.Interfaces; // CharacterRole
using _Game.Runtime.Board;               // BoardGrid, BoardSurface, GridProjector
using _Game.Runtime.Characters.Config;   // CharacterArchetype
using _Game.Runtime.Characters.Plugins;  // HealthPlugin, MovementPlugin (lane-based)
using _Game.Runtime.Characters.View;     // CharacterView, ICharacterView
using _Game.Utils;                        // GameObjectPool

namespace _Game.Runtime.Characters
{
    /// <summary>
    /// Spawns characters (defense/enemy) using pooling when possible.
    /// Attaches default plugins (health, lane-movement for enemies), then registers with CharacterSystem.
    /// </summary>
    public sealed class CharacterFactory
    {
        private readonly CharacterPoolRegistry _pools;
        private int _nextEntityId = 1;

        public CharacterFactory(CharacterPoolRegistry pools)
        {
            _pools = pools;
        }

        /// <summary>
        /// Spawn an entity and place its view at a world position.
        /// Sets logical Cell/Role on the entity.
        /// </summary>
        public CharacterEntity SpawnAtWorld(
            CharacterArchetype archetype,
            Vector3 worldPosition,
            Core.Cell cell,
            Transform parent,
            CharacterRole role)
        {
            var entity = Spawn(archetype, cell, parent, role);

            // Position the root at the requested world position
            var root = entity.View.Root;
            if (parent != null && root.parent != parent)
                root.SetParent(parent, worldPositionStays: true);

            root.position = worldPosition;
            return entity;
        }

        /// <summary>
        /// Spawn an entity at a logical Cell (caller may position the view later).
        /// </summary>
        public CharacterEntity Spawn(
            CharacterArchetype archetype,
            Core.Cell cell,
            Transform parent,
            CharacterRole role)
        {
            // 1) Acquire/instantiate the visual GameObject (pool if available)
            Transform root = GetOrCreateUnitRoot(archetype, parent);

            // 2) Ensure we have a view component implementing ICharacterView
            var view = root.GetComponent<ICharacterView>();
            if (view == null)
            {
                // CharacterView is your concrete view that implements ICharacterView
                var concrete = root.gameObject.GetComponent<CharacterView>() ?? root.gameObject.AddComponent<CharacterView>();
                view = concrete;
            }

            // 3) Build the entity with the correct constructor (id, archetype, view)
            var id = _nextEntityId++;
            var entity = new CharacterEntity(id, archetype, view)
            {
                Cell = cell,
                Role = role
            };

            // 4) Attach default plugins (health for all; movement for enemies)
            AttachDefaultPlugins(entity);

            // 5) Hand over to CharacterSystem so plugins tick each frame
            GameContext.Container.Resolve<CharacterSystem>().Register(entity);

            return entity;
        }

        /// <summary>
        /// Adds baseline plugins required for gameplay.
        /// </summary>
        private void AttachDefaultPlugins(CharacterEntity e)
        {
            // Everyone gets health
            e.AddPlugin(new HealthPlugin(e.Archetype.baseHealth));

            if (e.Role == CharacterRole.Enemy)
            {
                // Lane-based Movement: straight down the current column (no waypoints)
                var grid      = GameContext.Container.Resolve<BoardGrid>();
                var surface   = GameContext.Container.Resolve<BoardSurface>();
                var projector = GameContext.Container.Resolve<GridProjector>();

                if (e.Archetype.moveSpeed > 0f)
                {
                    e.AddPlugin(new MovementPlugin(
                        grid,
                        surface,
                        projector,
                        e.Archetype.moveSpeed  // blocks per second
                    ));
                }
            }
            else
            {
                // (Optional) attach your attack plugin(s) for defenses here.
                // Example if you already have one:
                // var targets = GameContext.Container.Resolve<TargetingService>();
                // e.AddPlugin(new RangedAttackPlugin(targets, e.Archetype.attackRate,
                //     Mathf.RoundToInt(e.Archetype.attackDamage),
                //     e.Archetype.attackRangeBlocks, e.Archetype.attackDirection));
            }
        }

        /// <summary>
        /// Gets a pooled unit root for the given archetype; falls back to Instantiate/New GO.
        /// </summary>
        private Transform GetOrCreateUnitRoot(CharacterArchetype archetype, Transform parent)
        {
            Transform root = null;

            // Try pooled unit first (if your registry is set up)
            var unitPool = _pools?.GetUnitPool(archetype, () =>
            {
                // If no pool exists yet, create one with a sensible default capacity
                var prefab = archetype.viewPrefab != null
                    ? archetype.viewPrefab
                    : new GameObject("CharacterView"); // fallback empty view go

                return new GameObjectPool(prefab, 8, parent);
            });

            if (unitPool != null)
            {
                var go = unitPool.Get();
                if (parent && go.transform.parent != parent)
                    go.transform.SetParent(parent, worldPositionStays: false);

                root = go.transform;
            }
            else
            {
                // No pooling available; just instantiate
                GameObject go;
                if (archetype.viewPrefab != null)
                {
                    go = Object.Instantiate(archetype.viewPrefab, parent, false);
                }
                else
                {
                    go = new GameObject("CharacterView");
                    if (parent) go.transform.SetParent(parent, worldPositionStays: false);
                }
                root = go.transform;
            }

            return root;
        }
    }
}
