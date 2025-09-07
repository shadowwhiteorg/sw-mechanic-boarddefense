using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Game.Runtime.Characters.Config;

namespace _Game.Systems.UI.Defense
{
    public sealed class DefenseCatalogueItemView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject selectedFrame;

        public int Index { get; private set; }
        public CharacterArchetype Archetype { get; private set; }

        public event System.Action<int, CharacterArchetype> OnClicked; 

        public void Bind(int index, CharacterArchetype a)
        {
            Index = index;
            Archetype = a;

            if (icon)  icon.sprite = a.icon;
            if (label) label.text  = string.IsNullOrWhiteSpace(a.displayName) ? a.name : a.displayName;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClicked?.Invoke(Index, Archetype));
        }

        public void SetSelected(bool selected)
        {
            if (selectedFrame) selectedFrame.SetActive(selected);
        }
    }
}