using System.Collections.Generic;
using _Game.Enums;
using _Game.Interfaces;
using _Game.Runtime.Characters.Config;
using _Game.Runtime.Core;

namespace _Game.Runtime.Characters
{
    public sealed class CharacterEntity
    {
        public int EntityId { get; }
        public CharacterArchetype Archetype { get; }
        public CharacterRole Role { get; internal set; }
        public Cell Cell { get; internal set; }
        public ICharacterView View { get; }

        public IReadOnlyList<ICharacterPlugin> Plugins => _plugins;

        private readonly List<ICharacterPlugin> _plugins = new();

        public CharacterEntity(int id, CharacterArchetype archetype, ICharacterView view)
        {
            EntityId = id;
            Archetype = archetype;
            View = view;
        }

        public void AddPlugin(ICharacterPlugin plugin)
        {
            if (plugin != null) _plugins.Add(plugin);
        }
    }
}