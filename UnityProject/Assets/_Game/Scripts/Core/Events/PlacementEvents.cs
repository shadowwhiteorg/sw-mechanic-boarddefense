using _Game.Interfaces;
using _Game.Runtime.Core;
using _Game.Runtime.Characters.Config;

namespace _Game.Core.Events
{
    /// <summary>Fired by UI when a defense archetype is chosen for placement.</summary>
    public readonly struct CharacterSelectedEvent : IGameEvent
    {
        public readonly CharacterArchetype Archetype;
        public CharacterSelectedEvent(CharacterArchetype archetype) { Archetype = archetype; }
    }

    /// <summary>Broadcast when placement mode is toggled (enter/exit).</summary>
    public readonly struct PlacementModeChangedEvent : IGameEvent
    {
        public readonly bool IsActive;
        public PlacementModeChangedEvent(bool active) { IsActive = active; }
    }

    /// <summary>Broadcast after a character is successfully placed on the grid.</summary>
    public readonly struct CharacterPlacedEvent : IGameEvent
    {
        public readonly CharacterArchetype Archetype;
        public readonly int EntityId;
        public readonly Cell Cell;
        public CharacterPlacedEvent(CharacterArchetype a, int id, Cell c)
        { Archetype = a; EntityId = id; Cell = c; }
    }
}