using System;
using _Game.Runtime.Characters.Config;
using UnityEngine;

namespace _Game.Runtime.Levels
{
    [Serializable]
    public struct DefenseStackEntry
    {
        public CharacterArchetype archetype;
        [Min(0)] public int count;
    }

    [Serializable]
    public struct EnemyStackEntry
    {
        public CharacterArchetype archetype;
        [Min(0)] public int count;
    }

}