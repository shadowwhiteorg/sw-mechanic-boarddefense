using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Runtime.Systems.UI
{
    [CreateAssetMenu(menuName = "_Game/UI/Screen Registry", fileName = "UiScreenRegistry")]
    public sealed class UiScreenRegistry : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            [Tooltip("Fully qualified type name: Namespace.ClassName, AssemblyName")]
            public string screenType;

            [Tooltip("Prefab that has the screen + view components.")]
            public GameObject prefab;
        }

        [SerializeField] private List<Entry> entries = new();

        private readonly Dictionary<Type, GameObject> _map = new();

        public void BuildCache()
        {
            _map.Clear();
            foreach (var e in entries)
            {
                if (string.IsNullOrWhiteSpace(e.screenType) || e.prefab == null) continue;
                var t = Type.GetType(e.screenType, throwOnError: false);
                if (t == null) { Debug.LogWarning($"[UiScreenRegistry] Type not found: {e.screenType}"); continue; }
                if (!_map.ContainsKey(t)) _map.Add(t, e.prefab);
            }
        }

        public bool TryGetPrefab(Type screenType, out GameObject prefab)
        {
            if (_map.Count == 0) BuildCache();
            return _map.TryGetValue(screenType, out prefab);
        }

        public GameObject GetPrefabOrNull(Type screenType)
        {
            TryGetPrefab(screenType, out var p);
            return p;
        }
    }
}