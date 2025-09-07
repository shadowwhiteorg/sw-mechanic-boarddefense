using UnityEngine;
using _Game.Runtime.Combat;

namespace _Game.Runtime.Characters.Config
{
    [CreateAssetMenu(menuName = "_Game/Characters/Archetype", fileName = "CharacterArchetype")]
    public sealed class CharacterArchetype : ScriptableObject
    {
        [Header("Identity")]
        public string displayName;
        public Sprite icon;

        [Header("View")]
        public GameObject viewPrefab;

        [Header("Base Stats")]
        [Min(1)]      public int   baseHealth   = 10;
        [Min(0)]      public float moveSpeed    = 0f;   // 0 for defenses

        

        [Tooltip("Weapon ScriptableObject carried by this character archetype.")]
        public WeaponConfig weapon;
    }
}