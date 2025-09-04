using UnityEngine;
using _Game.Interfaces;
using _Game.Runtime.Characters;
using _Game.Runtime.Characters.Config;
using _Game.Runtime.Core;

namespace _Game.Core.Events
{
    public readonly struct HoverCellChangedEvent : IGameEvent
    {
        public readonly Cell?   Cell;
        public readonly Vector3 World;

        public HoverCellChangedEvent(Cell? cell, Vector3 world)
        {
            Cell = cell; World = world;
        }

        public override string ToString()
            => Cell.HasValue ? $"HoverCellChanged(Cell={Cell.Value}, World={World})" : "HoverCellChanged(Cell=null)";
    }
    
    public readonly struct CharacterSelectedEvent : IGameEvent
    {
        public readonly CharacterArchetype Archetype;
        public CharacterSelectedEvent(CharacterArchetype archetype) { Archetype = archetype; }
    }

    public readonly struct PlacementModeChangedEvent : IGameEvent
    {
        public readonly bool IsActive;
        public PlacementModeChangedEvent(bool active) { IsActive = active; }
    }

    public readonly struct CharacterPlacedEvent : IGameEvent
    {
        public readonly CharacterArchetype Archetype;
        public readonly int EntityId;
        public readonly Cell Cell;
        public readonly bool IsEnemy;
        public CharacterPlacedEvent(CharacterArchetype a, int id, Cell c, bool isEnemy)
        { Archetype = a; EntityId = id; Cell = c; IsEnemy = isEnemy; }
    }
    public readonly struct CharacterDiedEvent : IGameEvent
    {
        public readonly CharacterEntity Entity;
        public CharacterDiedEvent(CharacterEntity entity) { Entity = entity; }
    }

    public readonly struct CharacterDespawnedEvent : IGameEvent
    {
        public readonly int EntityId;
        public CharacterDespawnedEvent(int entityId) { EntityId = entityId; }
    }
    
}