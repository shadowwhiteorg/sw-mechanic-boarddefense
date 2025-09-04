using UnityEngine;
using _Game.Interfaces;
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
}