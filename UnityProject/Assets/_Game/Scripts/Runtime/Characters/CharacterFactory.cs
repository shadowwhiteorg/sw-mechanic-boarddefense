using _Game.Enums;
using _Game.Interfaces;
using UnityEngine;
using _Game.Runtime.Core;
using _Game.Runtime.Characters.Config;
using _Game.Runtime.Characters.View;
using _Game.Runtime.Characters.Plugins;
using _Game.Utils;

namespace _Game.Runtime.Characters
{
    public sealed class CharacterFactory
    {
        private int _nextId = 1;
        private readonly CharacterPoolRegistry _pools;

        public CharacterFactory(CharacterPoolRegistry pools) { _pools = pools; }

        public CharacterEntity Spawn(
            CharacterArchetype archetype,
            Cell cell,
            Transform parent,
            CharacterRole role,
            Vector3[] enemyPath = null)
        {
            var pool = _pools.GetUnitPool(archetype, () => new GameObjectPool(archetype.viewPrefab, 8, parent));
            var go = pool.Get();
            var view = go.GetComponent<ICharacterView>() ?? go.AddComponent<CharacterView>();

            var id = _nextId++;
            var e = new CharacterEntity(id, archetype, view) { Cell = cell, Role = role };

            // Common plugins
            e.AddPlugin(new HealthPlugin(archetype.baseHealth));

            // Role-specific plugins
            if (role == CharacterRole.Enemy)
            {
                if (archetype.moveSpeed > 0f && enemyPath != null && enemyPath.Length > 0)
                    e.AddPlugin(new MovementPlugin(archetype.moveSpeed, enemyPath));
                e.AddPlugin(new MeleeAttackPlugin(archetype.attackRate, Mathf.CeilToInt(archetype.attackDamage)));
            }
            else // Defense
            {
                e.AddPlugin(new MeleeAttackPlugin(archetype.attackRate, Mathf.CeilToInt(archetype.attackDamage)));
            }

            foreach (var p in e.Plugins) p.OnSpawn(e);

            view.Bind(e);
            view.SetGhostVisual(false, true);
            view.Show();

            return e;
        }

        public GameObject GetPreview(CharacterArchetype a, Transform parent)
        {
            var pool = _pools.GetPreviewPool(a, () => new GameObjectPool(a.viewPrefab, 1, parent));
            var go = pool.Get();
            var view = go.GetComponent<ICharacterView>() ?? go.AddComponent<CharacterView>();
            view.SetGhostVisual(true, false);
            view.Show();
            return go;
        }

        public void ReturnPreview(CharacterArchetype a, GameObject instance)
        {
            var pool = _pools.GetPreviewPool(a, () => new GameObjectPool(a.viewPrefab, 1, instance.transform.parent));
            pool.Return(instance);
        }
    }
}
