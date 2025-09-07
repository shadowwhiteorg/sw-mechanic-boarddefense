using _Game.Interfaces;
using _Game.Runtime.Core;
using _Game.Runtime.Characters.Config;
using _Game.Runtime.Selection;

namespace _Game.Core.Events
{
    public readonly struct CharacterSelectedEvent : IGameEvent
    {
        public readonly CharacterArchetype Archetype;
        public CharacterSelectedEvent(CharacterArchetype archetype) { Archetype = archetype; }
    }

    public struct PlacementModeChangedEvent : IGameEvent
    {
        public bool IsActive;
        public PlacementModeChangedEvent(bool active) { IsActive = active; }
    }

    public readonly struct CharacterPlacedEvent : IGameEvent
    {
        public readonly CharacterArchetype Archetype;
        public readonly int EntityId;
        public readonly Cell Cell;
        public CharacterPlacedEvent(CharacterArchetype a, int id, Cell c)
        { Archetype = a; EntityId = id; Cell = c; }
    }
    
    public readonly struct SelectableSpawnedEvent : IGameEvent
    {
        public readonly SelectableCharacterView View;
        public SelectableSpawnedEvent(SelectableCharacterView view) => View = view;
    }
}