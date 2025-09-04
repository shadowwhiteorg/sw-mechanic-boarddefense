// --- FILE: LevelRuntimeConfig.cs ---
using System.Collections.Generic;
using UnityEngine;
using _Game.Runtime.Characters.Config;

namespace _Game.Runtime.Levels
{
    /// <summary>Snapshot of the active level for runtime systems.</summary>
    public sealed class LevelRuntimeConfig
    {
        public string LevelId { get; }
        public IReadOnlyList<CharacterArchetype> AllowedDefenseArchetypes { get; }
        public IReadOnlyList<EnemyWave> Waves { get; }
        public IReadOnlyList<PathAsset> Paths { get; }

        public LevelRuntimeConfig(LevelData data)
        {
            LevelId = data.levelId;
            AllowedDefenseArchetypes = data.allowedDefenseArchetypes;
            Waves = data.waves;
            Paths = data.paths;
        }
    }
}