using System.Collections.Generic;
using UnityEngine;
using _Game.Runtime.Characters.Config;

namespace _Game.Runtime.Levels
{
    public sealed class LevelRuntimeConfig
    {
        public readonly LevelData Source;

        private readonly Dictionary<CharacterArchetype, int> _defenseRemaining = new();
        public IReadOnlyDictionary<CharacterArchetype, int> DefenseRemaining => _defenseRemaining;

        private readonly Dictionary<CharacterArchetype, int> _enemyRemaining = new();
        public IReadOnlyDictionary<CharacterArchetype, int> EnemyRemaining => _enemyRemaining;
        
        private readonly Dictionary<CharacterArchetype, int> _defenseStock;

        public LevelRuntimeConfig(LevelData data)
        {
            Source = data;

            if (data != null)
            {
                if (data.defenses != null)
                    foreach (var d in data.defenses)
                        if (d.archetype) _defenseRemaining[d.archetype] = Mathf.Max(0, d.count);

                if (data.enemies != null)
                    foreach (var e in data.enemies)
                        if (e.archetype) _enemyRemaining[e.archetype] = Mathf.Max(0, e.count);
            }
        }

        public int GetDefenseRemaining(CharacterArchetype a)
            => (a && _defenseRemaining.TryGetValue(a, out var rem)) ? rem : 0;

        public int ConsumeDefenseOne(CharacterArchetype a)
        {
            if (!a) return 0;
            if (!_defenseRemaining.TryGetValue(a, out var rem)) return 0;
            rem = Mathf.Max(0, rem - 1);
            _defenseRemaining[a] = rem;
            return rem;
        }
        
        public int GetRemainingDefense(CharacterArchetype a)
            => (a != null && _defenseStock.TryGetValue(a, out var n)) ? Mathf.Max(0, n) : 0;

        public bool TryConsumeDefense(CharacterArchetype a)
        {
            if (a == null) return false;
            if (_defenseStock.TryGetValue(a, out var n) && n > 0)
            {
                _defenseStock[a] = n - 1;
                return true;
            }
            return false;
        }
    }
}