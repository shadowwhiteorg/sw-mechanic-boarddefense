// Assets/_Game/Scripts/Runtime/Characters/CharacterFactory.cs
using UnityEngine;
using _Game.Core;                         // GameContext
using _Game.Enums;
using _Game.Interfaces; // CharacterRole
using _Game.Runtime.Board;                // BoardGrid, BoardSurface, GridProjector
using _Game.Runtime.Characters.Config;    // CharacterArchetype
using _Game.Runtime.Characters.Plugins;   // HealthPlugin, WeaponPlugin, RangedAttackPlugin, MovementPlugin
using _Game.Runtime.Characters.View;      // CharacterView, ICharacterView
using _Game.Runtime.Combat;               // WeaponConfig
using _Game.Runtime.Core;                 // Cell
using _Game.Utils;                        // GameObjectPool

namespace _Game.Runtime.Characters
{
    /// Spawns characters, composes their plugins, registers them for ticking,
    /// and keeps the repository mapping accurate at spawn time.
    public sealed class CharacterFactory
    {
        private readonly CharacterPoolRegistry _pools;
        private int _nextEntityId = 1;

        public CharacterFactory(CharacterPoolRegistry pools)
        {
            _pools = pools;
        }

        public CharacterEntity Spawn(CharacterArchetype archetype, Cell cell, Transform parent, CharacterRole role)
        {
            // Create / fetch a pooled view root.
            Transform root = GetOrCreateUnitRoot(archetype, parent);

            // Ensure a concrete view exists.
            var view =
                root.GetComponent<ICharacterView>() ??
                (root.GetComponent<CharacterView>() ?? root.gameObject.AddComponent<CharacterView>());

            var id = _nextEntityId++;
            var entity = new CharacterEntity(id, archetype, view)
            {
                Cell = cell,
                Role = role
            };

            // Attach gameplay plugins.
            AttachPlugins(entity);

            // Add to repository & register to character system for ticking.
            var repo = GameContext.Container.Resolve<CharacterRepository>();
            repo.Add(entity, cell);

            var charSystem = GameContext.Container.Resolve<CharacterSystem>();
            charSystem.Register(entity);

            return entity;
        }

        public CharacterEntity SpawnAtWorld(CharacterArchetype archetype, Vector3 worldPosition, Cell cell, Transform parent, CharacterRole role)
        {
            var e = Spawn(archetype, cell, parent, role);
            var root = e.View?.Root;
            if (parent && root && root.parent != parent)
                root.SetParent(parent, true);
            if (root) root.position = worldPosition;
            return e;
        }

        private void AttachPlugins(CharacterEntity e)
        {
            // Every unit has health.
            e.AddPlugin(new HealthPlugin(e.Archetype.baseHealth));

            if (e.Role == CharacterRole.Defense)
            {
                // Defense = cadence (Weapon) + targeting (Ranged).
                var events   = GameContext.Events;
                var grid     = GameContext.Container.Resolve<BoardGrid>();
                var repo     = GameContext.Container.Resolve<CharacterRepository>();

                WeaponConfig wcfg = e.Archetype.weapon;

                var weapon = new WeaponPlugin(
                    events,
                    grid,
                    e.Archetype.weapon.attackDirection,
                    e.Archetype.weapon.rangeBlocks,
                    wcfg
                );

                var ranged = new RangedAttackPlugin(
                    repo,
                    grid,
                    e.Archetype.weapon.attackDirection,
                    e.Archetype.weapon.rangeBlocks
                );

                ranged.BindWeapon(weapon);

                e.AddPlugin(weapon);
                e.AddPlugin(ranged);
            }
            else // Enemy
            {
                var grid      = GameContext.Container.Resolve<BoardGrid>();
                var surface   = GameContext.Container.Resolve<BoardSurface>();
                var projector = GameContext.Container.Resolve<GridProjector>();
                var repo      = GameContext.Container.Resolve<CharacterRepository>();
                
                var ranged = new RangedAttackPlugin(
                    repo,
                    grid,
                    e.Archetype.weapon.attackDirection,
                    e.Archetype.weapon.rangeBlocks
                );
                WeaponConfig wcfg = e.Archetype.weapon;

                var weapon = new WeaponPlugin(
                    GameContext.Events,
                    grid,
                    e.Archetype.weapon.attackDirection,
                    e.Archetype.weapon.rangeBlocks,
                    wcfg
                );
                e.AddPlugin(weapon);
                e.AddPlugin(ranged);
                
                if (e.Archetype.moveSpeed > 0f)
                {
                    e.AddPlugin(new MovementPlugin(
                        grid,
                        surface,
                        projector,
                        e.Archetype.moveSpeed,
                        repo
                    ));
                }
            }
        }

        private Transform GetOrCreateUnitRoot(CharacterArchetype archetype, Transform parent)
        {
            // Prefer pooled creation when registry is present.
            if (_pools != null)
            {
                var unitPool = _pools.GetUnitPool(archetype, () =>
                {
                    var prefab = archetype.viewPrefab != null
                        ? archetype.viewPrefab
                        : new GameObject("CharacterView");
                    return new GameObjectPool(prefab, initialSize: 8, parent: parent);
                });

                var go = unitPool.Get();
                if (parent && go.transform.parent != parent)
                    go.transform.SetParent(parent, false);
                return go.transform;
            }

            // Fallback: direct instantiate.
            var instance = archetype.viewPrefab
                ? Object.Instantiate(archetype.viewPrefab, parent, false)
                : new GameObject("CharacterView");
            return instance.transform;
        }
    }
}
