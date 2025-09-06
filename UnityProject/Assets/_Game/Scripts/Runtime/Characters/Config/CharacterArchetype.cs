using _Game.Enums;
using UnityEngine;
using _Game.Runtime.Combat;

namespace _Game.Runtime.Characters.Config
{
    [CreateAssetMenu(menuName = "_Game/Characters/Archetype", fileName = "CharacterArchetype")]
    public sealed class CharacterArchetype : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        public Sprite icon;
        public bool isEnemy;

        [Header("View")]
        public GameObject viewPrefab;

        [Header("Base Stats")]
        [Min(1)]      public int   baseHealth   = 10;
        [Min(0)]      public float moveSpeed    = 0f;   // 0 for defenses

        [Header("Combat")]
        [Min(0)]      public int   attackRangeBlocks = 1;             // grid blocks
        public AttackDirection     attackDirection    = AttackDirection.Forward;

        [Tooltip("Weapon ScriptableObject carried by this character archetype.")]
        public WeaponConfig weapon;
    }
}