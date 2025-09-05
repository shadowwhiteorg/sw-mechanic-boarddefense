using System.Collections.Generic;
using _Game.Core.Events;
using UnityEngine;
using _Game.Enums;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Core;
using _Game.Runtime.Placement;

namespace _Game.Runtime.Selection
{
    public sealed class CharacterSelectionSystem : IUpdatableSystem
    {
        private readonly IRayProvider _rayProvider;
        private readonly GridProjector _projector;
        private readonly BoardGrid _grid;
        private readonly BoardSurface _surface;
        private readonly CharacterFactory _factory;
        private readonly CharacterRepository _repo;
        private readonly PlacementValidator _validator;
        private readonly Transform _placedParent;
        private readonly List<SelectableCharacterView> _selectables;
        private readonly IEventBus _events;
        private readonly float _dragLift;

        private SelectableCharacterView _selected;
        private bool _dragging;
        private Cell? _activeCell;
        private Vector3 _cursorWorld;

        public CharacterSelectionSystem(
            IRayProvider rayProvider,
            GridProjector projector,
            BoardGrid grid,
            BoardSurface surface,
            CharacterFactory factory,
            CharacterRepository repo,
            PlacementValidator validator,
            Transform placedParent,
            List<SelectableCharacterView> selectables,
            IEventBus events,
            float dragLift = 0.01f)
        {
            _rayProvider  = rayProvider;
            _projector    = projector;
            _grid         = grid;
            _surface      = surface;
            _factory      = factory;
            _repo         = repo;
            _validator    = validator;
            _placedParent = placedParent;
            _selectables  = selectables;
            _events       = events;
            _dragLift     = Mathf.Max(0f, dragLift);

            _events.Subscribe<HoverCellChangedEvent>(OnHoverCellChanged);
        }

        public void Tick()
        {
            if (!_dragging && Input.GetMouseButtonDown(0))
                TryBeginDrag();

            if (_dragging && _selected != null)
            {
                var ray = _rayProvider.PointerToRay(Input.mousePosition);

                if (InputProjectionMath.TryRayPlane(
                        ray,
                        _surface.WorldPlanePoint,
                        _surface.WorldPlaneNormal,
                        out var hitWorld))
                {
                    _cursorWorld = hitWorld;

                    // Free move with tiny lift to prevent z-fighting
                    var lifted = hitWorld + _surface.WorldPlaneNormal * _dragLift;
                    _selected.transform.position = lifted;

                    if (Input.GetMouseButtonUp(0))
                        TryEndDrag();
                }
                else
                {
                    if (Input.GetMouseButtonUp(0))
                        CancelDrag();
                }
            }
        }

        private void TryBeginDrag()
        {
            var ray = _rayProvider.PointerToRay(Input.mousePosition);

            SelectableCharacterView best = null;
            float bestT = float.PositiveInfinity;

            for (int i = 0; i < _selectables.Count; i++)
            {
                var view = _selectables[i];
                if (!view) continue;

                if (view.TryRaycastBounds(ray, out float t) && t < bestT)
                {
                    bestT = t;
                    best = view;
                }
            }

            if (best != null)
            {
                _selected = best;
                _dragging = true;
            }
        }

        private void TryEndDrag()
        {
            // Prefer the currently "activated" (hovered) cell
            if (_activeCell.HasValue && _validator.IsValid(_activeCell.Value))
            {
                PlaceAndFinalize(_activeCell.Value);
                return;
            }

            if (_projector.TryWorldToCell(_cursorWorld, out var cell) && _validator.IsValid(cell))
            {
                PlaceAndFinalize(cell);
                return;
            }

            CancelDrag();
        }

        private void PlaceAndFinalize(Cell cell)
        {
            _dragging = false;

            // Snap to grid center
            var worldCenter = _projector.CellToWorldCenter(cell);

            // Spawn at worldCenter so we never inherit prefab/selection offsets
            var entity = _factory.SpawnAtWorld(
                _selected.Archetype,
                worldCenter,
                cell,
                _placedParent,
                CharacterRole.Defense);

            _repo.Add(entity, cell);

            // Remove selection model from pool and destroy it
            // TODO: Use Object Pool
            _selectables.Remove(_selected);
            Object.Destroy(_selected.gameObject);
            _selected = null;
        }

        private void CancelDrag()
        {
            _dragging = false;
            if (_selected != null)
            {
                _selected.ResetPosition(); // remains selectable after failure
                _selected = null;
            }
        }

        private void OnHoverCellChanged(HoverCellChangedEvent e)
        {
            _activeCell = e.Cell; // may be null
        }
    }
}
