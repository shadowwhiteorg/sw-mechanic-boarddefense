using System.Collections.Generic;
using UnityEngine;

namespace _Game.Runtime.Levels
{
    [CreateAssetMenu(menuName = "_Game/Levels/LevelCatalogue", fileName = "LevelCatalogue")]
    public sealed class LevelCatalogue : ScriptableObject
    {
        [SerializeField] private List<LevelData> levels = new();

        public LevelData GetById(string id)
        {
            for (int i = 0; i < levels.Count; i++)
                if (levels[i] && levels[i].id == id) return levels[i];
            return null;
        }

        public LevelData GetByLevelNr(int levelNr)
        {
            var index = (levelNr % levels.Count + levels.Count) % levels.Count;
            return levels[index];
        }
    }
}