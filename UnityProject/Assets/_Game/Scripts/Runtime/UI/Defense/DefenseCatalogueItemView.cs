// --- FILE: DefenseCatalogueItemView.cs ---
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Game.Runtime.Characters.Config;

namespace _Game.Systems.UI.Defense
{
    public sealed class DefenseCatalogueItemView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button           _button;
        [SerializeField] private Image            _icon;
        [SerializeField] private TextMeshProUGUI  _label;
        [SerializeField] private GameObject       _selectedFrame;

        public int Index { get; private set; }
        public CharacterArchetype Archetype { get; private set; }

        public event System.Action<int, CharacterArchetype> OnClicked; // (index, archetype)

        public void Bind(int index, CharacterArchetype a)
        {
            Index = index;
            Archetype = a;

            if (_icon)  _icon.sprite = a.icon;
            if (_label) _label.text  = string.IsNullOrWhiteSpace(a.displayName) ? a.name : a.displayName;

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => OnClicked?.Invoke(Index, Archetype));
        }

        public void SetSelected(bool selected)
        {
            if (_selectedFrame) _selectedFrame.SetActive(selected);
        }
    }
}