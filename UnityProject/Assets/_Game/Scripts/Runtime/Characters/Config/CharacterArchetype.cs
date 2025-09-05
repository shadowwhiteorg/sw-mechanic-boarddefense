using UnityEngine;

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
        [Min(1)] public int baseHealth = 10;
        [Min(0)] public float attackDamage = 1f;
        [Min(0.05f)] public float attackRate = 1f; // per second
        [Min(0)] public float moveSpeed = 0f; // 0 for defenses
    }
}