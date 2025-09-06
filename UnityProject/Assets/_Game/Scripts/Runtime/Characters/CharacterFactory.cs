using UnityEngine;

using _Game.Core;                         // GameContext (DI access)
using _Game.Enums;                        // CharacterRole
using _Game.Interfaces;
using _Game.Runtime.Board;               // BoardGrid, BoardSurface, GridProjector
using _Game.Runtime.Characters.Config;   // CharacterArchetype
using _Game.Runtime.Characters.Plugins;  // HealthPlugin, MovementPlugin, WeaponPlugin
using _Game.Runtime.Characters.View;     // CharacterView, ICharacterView
using _Game.Runtime.Combat;              // WeaponConfig (optional)
using _Game.Utils;                       // GameObjectPool

namespace _Game.Runtime.Characters
{
    public sealed class CharacterFactory
    {
        private readonly CharacterPoolRegistry _pools;
        private int _nextEntityId = 1;

        public CharacterFactory(CharacterPoolRegistry pools) { _pools = pools; }

        public CharacterEntity Spawn(CharacterArchetype archetype, Core.Cell cell, Transform parent, CharacterRole role, WeaponConfig optionalWeapon = null)
        {
            Transform root = GetOrCreateUnitRoot(archetype, parent);

            var view = root.GetComponent<ICharacterView>();
            if (view == null)
                view = (root.gameObject.GetComponent<CharacterView>() ?? root.gameObject.AddComponent<CharacterView>());

            var id = _nextEntityId++;
            var entity = new CharacterEntity(id, archetype, view) { Cell = cell, Role = role };

            AttachDefaultPlugins(entity, optionalWeapon);
            GameContext.Container.Resolve<CharacterSystem>().Register(entity);
            return entity;
        }

        public CharacterEntity SpawnAtWorld(CharacterArchetype archetype, Vector3 worldPosition, Core.Cell cell, Transform parent, CharacterRole role, WeaponConfig optionalWeapon = null)
        {
            var e = Spawn(archetype, cell, parent, role, optionalWeapon);
            var root = e.View.Root;
            if (parent != null && root.parent != parent)
                root.SetParent(parent, true);
            root.position = worldPosition;
            return e;
        }

        private void AttachDefaultPlugins(CharacterEntity e, WeaponConfig weaponCfg)
        {
            e.AddPlugin(new HealthPlugin(e.Archetype.baseHealth));

            if (e.Role == CharacterRole.Defense)
            {
                var events = GameContext.Container.Resolve<_Game.Interfaces.IEventBus>();
                var repo   = GameContext.Container.Resolve<CharacterRepository>();
                var grid   = GameContext.Container.Resolve<BoardGrid>();

                if (weaponCfg != null)
                {
                    e.AddPlugin(new WeaponPlugin(events, repo, grid, e.Archetype.attackDirection, e.Archetype.attackRangeBlocks, weaponCfg));
                }
                else
                {
                    e.AddPlugin(new WeaponPlugin(
                        events, repo, grid,
                        e.Archetype.attackDirection,
                        e.Archetype.attackRangeBlocks,
                        fireRate: e.Archetype.attackRate,
                        damage:   Mathf.RoundToInt(e.Archetype.attackDamage),
                        projectileMode: true,
                        projectileSpeed: 8f,
                        pierceCount: 0,
                        splashRadius: 0f,
                        muzzleOffset: Vector3.zero));
                }
            }
            else
            {
                var grid      = GameContext.Container.Resolve<BoardGrid>();
                var surface   = GameContext.Container.Resolve<BoardSurface>();
                var projector = GameContext.Container.Resolve<GridProjector>();

                if (e.Archetype.moveSpeed > 0f)
                    e.AddPlugin(new MovementPlugin(grid, surface, projector, e.Archetype.moveSpeed));
            }
        }

        private Transform GetOrCreateUnitRoot(CharacterArchetype archetype, Transform parent)
        {
            Transform root = null;

            var unitPool = _pools?.GetUnitPool(archetype, () =>
            {
                var prefab = archetype.viewPrefab != null ? archetype.viewPrefab : new GameObject("CharacterView");
                return new GameObjectPool(prefab, 
                    initialSize: 8, parent: parent);
            });

            if (unitPool != null)
            {
                var go = unitPool.Get();
                if (parent && go.transform.parent != parent)
                    go.transform.SetParent(parent, false);
                root = go.transform;
            }
            else
            {
                GameObject go = archetype.viewPrefab ? Object.Instantiate(archetype.viewPrefab, parent, false) : new GameObject("CharacterView");
                if (!archetype.viewPrefab && parent) go.transform.SetParent(parent, false);
                root = go.transform;
            }
            return root;
        }
    }
}
