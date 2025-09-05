using System;
using System.Collections.Generic;
using UnityEngine;
using _Game.Interfaces;
using _Game.Runtime.Core;
using _Game.Runtime.Board;
using _Game.Core.Events;
using _Game.Utils;

namespace _Game.Runtime.Visuals
{
    public sealed class GridVisualsService : IDisposable
    {
        private readonly BoardGrid _grid;
        private readonly BoardSurface _surface;
        private readonly GridProjector _projector;
        private readonly IEventBus _events;

        private readonly GameObjectPool _placeablePool;
        private readonly Transform _parent;
        private readonly Vector3 _lift; // nudge above surface to prevent z-fighting

        private readonly Dictionary<Cell, GameObject> _markers = new();

        private readonly GameObject _hoverGO;
        private bool _disposed;

        public GridVisualsService(
            BoardGrid grid,
            BoardSurface surface,
            GridProjector projector,
            IEventBus events,
            GameObjectPool placeablePool,
            GameObject hoverInstance,
            Transform parent,
            float liftDistance = 0.01f)
        {
            _grid         = grid;
            _surface      = surface;
            _projector    = projector;
            _events       = events;
            _placeablePool= placeablePool;
            _parent       = parent;
            _hoverGO      = hoverInstance;
            _lift         = _surface.WorldPlaneNormal * liftDistance;

            _hoverGO.SetActive(false);
            BuildAllPlaceableMarkers();

            _events.Subscribe<HoverCellChangedEvent>(OnHoverChanged);
        }

        private void BuildAllPlaceableMarkers()
        {
            for (int r = 0; r < _grid.Size.Rows; r++)
            {
                for (int c = 0; c < _grid.Size.Cols; c++)
                {
                    var cell = new Cell(r, c);
                    if (!_grid.IsDefensePlacementAllowed(cell)) continue;

                    var go = _placeablePool.Get();
                    go.transform.SetParent(_parent, worldPositionStays: false);

                    var p = _projector.CellToWorldCenter(cell) + _lift;
                    go.transform.position = p;

                    _markers[cell] = go;
                }
            }
        }

        private void OnHoverChanged(HoverCellChangedEvent e)
        {
            if (!e.Cell.HasValue)
            {
                _hoverGO.SetActive(false);
                return;
            }

            var cell = e.Cell.Value;
            if (!_grid.IsDefensePlacementAllowed(cell))
            {
                _hoverGO.SetActive(false);
                return;
            }

            var p = _projector.CellToWorldCenter(cell) + _lift;
            if (!_hoverGO.activeSelf) _hoverGO.SetActive(true);
            _hoverGO.transform.position = p;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _events.Unsubscribe<HoverCellChangedEvent>(OnHoverChanged);

            // Return markers to pool
            foreach (var kv in _markers)
            {
                var go = kv.Value;
                if (go != null) _placeablePool.Return(go);
            }
            _markers.Clear();

            if (_hoverGO != null) _hoverGO.SetActive(false);
        }
    }
}
