// Assets/_Game/Scripts/Systems/UI/UiService.cs
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Runtime.Systems.UI
{
    /// <summary>
    /// Minimal UI service that shows/hides screen prefabs.
    /// - Lazily instantiates each prefab once under screensParent (or this GameObject).
    /// - Show(prefab) -> ensures instance exists, sets it active.
    /// - HideAll()    -> deactivates all instantiated screens.
    /// </summary>
    public sealed class UiService : MonoBehaviour
    {
        [SerializeField] private Transform screensParent;   // optional; will be created if null

        // prefab -> instance
        private readonly Dictionary<GameObject, GameObject> _instances = new();

        private void Awake()
        {
            if (!screensParent)
            {
                var root = new GameObject("Screens");
                root.transform.SetParent(transform, false);
                screensParent = root.transform;
            }
        }

        /// <summary>Show (activate) a screen from a prefab. Instantiates once and reuses.</summary>
        public GameObject Show(GameObject screenPrefab)
        {
            if (screenPrefab == null)
            {
                Debug.LogWarning("[UiService] Show called with null prefab.");
                return null;
            }

            if (!_instances.TryGetValue(screenPrefab, out var instance) || instance == null)
            {
                instance = Instantiate(screenPrefab, screensParent, false);
                instance.name = screenPrefab.name;
                _instances[screenPrefab] = instance;
            }

            instance.SetActive(true);
            return instance;
        }

        /// <summary>Hide (deactivate) all screens previously shown via this service.</summary>
        public void HideAll()
        {
            foreach (var kv in _instances)
            {
                if (kv.Value) kv.Value.SetActive(false);
            }
        }
    }
}