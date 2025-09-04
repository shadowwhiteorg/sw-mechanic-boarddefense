// --- FILE: DefenseCatalogueView.cs ---
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Systems.UI.Defense
{
    /// <summary>Renders the catalogue and selection state.</summary>
    public sealed class DefenseCatalogueView : BaseUIView
    {
        [Header("List")]
        [SerializeField] private Transform itemsRoot;
        [SerializeField] private DefenseCatalogueItemView itemPrefab;

        [Header("Optional")]
        [SerializeField] private GameObject placementHint;

        private readonly List<DefenseCatalogueItemView> _spawned = new();
        public event System.Action<int> OnItemClicked;

        protected override void OnBind()
        {
            // Bound once when View.Bind(Model) is called by the Screen (per BaseUIView).
        }

        protected override void OnViewUpdated()
        {
            var m = (DefenseCatalogueModel)Model;

            // build / trim list to match data
            var need = m.Items?.Count ?? 0;
            while (_spawned.Count < need)
            {
                var v = Instantiate(itemPrefab, itemsRoot);
                int capturedIndex = _spawned.Count;
                v.OnClicked += (idx, _) => OnItemClicked?.Invoke(idx);
                _spawned.Add(v);
            }
            while (_spawned.Count > need)
            {
                var last = _spawned[_spawned.Count - 1];
                if (last) Destroy(last.gameObject);
                _spawned.RemoveAt(_spawned.Count - 1);
            }

            // bind & selection visuals
            for (int i = 0; i < _spawned.Count; i++)
            {
                var v = _spawned[i];
                if (!v) continue;
                v.gameObject.SetActive(true);
                v.Bind(i, m.Items?[i]);
                v.SetSelected(i == m.SelectedIndex);
            }

            if (placementHint) placementHint.SetActive(m.PlacementModeActive);
        }
    }
}
