using System.Collections.Generic;
using UnityEngine;

namespace _Game.Runtime.Levels
{

    [CreateAssetMenu(menuName = "_Game/Levels/LevelData", fileName = "LevelData")]
    public sealed class LevelData : ScriptableObject
    {
        [Header("ID")]
        public string id = "Level-1";

        [Header("Defense counts (selectable & placeable)")]
        public List<DefenseStackEntry> defenses = new();

        [Header("Enemy counts (optional, kept for future spawners)")]
        public List<EnemyStackEntry> enemies = new();
    }
}