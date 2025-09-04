using System.Collections.Generic;
using _Game.Runtime.Characters.Config;
using _Game.Utils;

namespace _Game.Runtime.Characters
{
    public sealed class CharacterPoolRegistry
    {
        private readonly Dictionary<CharacterArchetype, GameObjectPool> _unitPools = new();
        private readonly Dictionary<CharacterArchetype, GameObjectPool> _previewPools = new();

        public GameObjectPool GetUnitPool(CharacterArchetype a, System.Func<GameObjectPool> factory)
        {
            if (!_unitPools.TryGetValue(a, out var pool))
                _unitPools[a] = pool = factory();
            return pool;
        }

        public GameObjectPool GetPreviewPool(CharacterArchetype a, System.Func<GameObjectPool> factory)
        {
            if (!_previewPools.TryGetValue(a, out var pool))
                _previewPools[a] = pool = factory();
            return pool;
        }
    }
}