using _Game.Core.Events;
using _Game.Interfaces;
using _Game.Runtime.Core;
using _Game.Runtime.Board;
using UnityEngine;

namespace _Game.Runtime.Systems
{
    public sealed class PointerHoverSystem : IUpdatableSystem
    {
        private readonly IRayProvider _rayProvider;
        private readonly BoardSurface _surface;
        private readonly GridProjector _projector;
        private readonly IEventBus _events;

        private Cell?   _lastCell;
        private Vector3 _lastWorld;

        public PointerHoverSystem(
            IRayProvider rayProvider,
            BoardSurface surface,
            GridProjector projector,
            IEventBus events)
        {
            _rayProvider = rayProvider;
            _surface     = surface;
            _projector   = projector;
            _events      = events;
        }

        public void Tick()
        {
            var ray = _rayProvider.PointerToRay(Input.mousePosition);

            if (InputProjectionMath.TryRayPlane(ray, _surface.WorldPlanePoint, _surface.WorldPlaneNormal, out var hit))
            {
                if (_projector.TryWorldToCell(hit, out var cell))
                {
                    if (!_lastCell.HasValue || !_lastCell.Value.Equals(cell) || (hit - _lastWorld).sqrMagnitude > 0.0001f)
                    {
                        _lastCell  = cell;
                        _lastWorld = hit;
                        _events.Fire(new HoverCellChangedEvent(cell, hit));
                    }
                }
                else if (_lastCell.HasValue)
                {
                    _lastCell = null;
                    _events.Fire(new HoverCellChangedEvent(null, hit));
                }
            }
            else if (_lastCell.HasValue)
            {
                _lastCell = null;
                _events.Fire(new HoverCellChangedEvent(null, default));
            }
        }
    }
}
