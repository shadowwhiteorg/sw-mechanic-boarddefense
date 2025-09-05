using System;
using System.Collections.Generic;
using UnityEngine;
using _Game.Runtime.Characters.Config;

namespace _Game.Runtime.Levels
{
    [CreateAssetMenu(menuName = "_Game/Levels/Catalogue", fileName = "LevelCatalogue")]
    public sealed class LevelCatalogue : ScriptableObject
    {
        public List<LevelData> levels = new();
        public LevelData GetById(string id) => levels.Find(l => l.levelId == id);
    }

    [Serializable]
    public sealed class LevelData
    {
        public string levelId;

        [Header("Defenses enabled for this level")]
        public CharacterArchetype[] allowedDefenseArchetypes;

        [Header("Enemy Waves")]
        public List<EnemyWave> waves = new(); 

        [Header("Enemy Paths")]
        public PathAsset[] paths; 
    }

    [Serializable]
    public sealed class EnemyWave
    {
        public CharacterArchetype enemyArchetype; // isEnemy = true
        public int count = 5;
        public float spawnInterval = 1.0f;
        public int pathIndex = 0; // which path to use
        public float delayBefore = 2.0f; // delay before this wave starts
    }

    [CreateAssetMenu(menuName = "_Game/Levels/Path", fileName = "PathAsset")]
    public sealed class PathAsset : ScriptableObject
    {
        public Transform[] waypoints;
    }
}