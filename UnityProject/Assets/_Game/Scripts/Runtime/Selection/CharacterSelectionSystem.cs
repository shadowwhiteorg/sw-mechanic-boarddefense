using System.Collections.Generic;
using UnityEngine;

using _Game.Enums;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Core;
using _Game.Runtime.Placement;

namespace _Game.Runtime.Selection
{
    /// <summary>
    /// NO-PHYSICS selection + drag-to-place:
    /// - MouseDown: pick the closest selectable whose Renderer.bounds AABB intersects the pointer ray.
    /// - While held: project pointer onto the BoardSurface plane, snap to grid cell center, show selection there.
    /// - MouseUp: place if valid; else snap back to initial position.
    /// Rotation-proof (uses BoardSurface.WorldPlanePoint/WorldPlaneNormal).
    /// </summary>
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
        private readonly IReadOnlyList<SelectableCharacterView> _selectables;

        private SelectableCharacterView _selected;
        private bool _dragging;

        public CharacterSelectionSystem(
            IRayProvider rayProvider,
            GridProjector projector,
            BoardGrid grid,
            BoardSurface surface,                  // << use board's canonical plane
            CharacterFactory factory,
            CharacterRepository repo,
            PlacementValidator validator,
            Transform placedParent,
            IReadOnlyList<SelectableCharacterView> selectables)
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
        }

        public void Tick()
        {
            if (!_dragging && Input.GetMouseButtonDown(0))
                TryBeginDrag();

            if (_dragging && _selected != null)
            {
                var ray = _rayProvider.PointerToRay(Input.mousePosition);

                // Project pointer to board plane → world point → grid cell
                if (InputProjectionMath.TryRayPlane(
                        ray,
                        _surface.WorldPlanePoint,
                        _surface.WorldPlaneNormal,
                        out var hitWorld)
                    && _projector.TryWorldToCell(hitWorld, out var cell))
                {
                    var centerWorld = _projector.CellToWorldCenter(cell);
                    _selected.transform.position = centerWorld;

                    if (Input.GetMouseButtonUp(0))
                        EndDrag(cell);
                }
                else
                {
                    // No valid cell under pointer — dropping cancels.
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

            // Bounds-based picking (no colliders, no Physics.Raycast)
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

        private void EndDrag(Cell cell)
        {
            _dragging = false;

            if (_validator.IsValid(cell))
            {
                var entity = _factory.Spawn(_selected.Archetype, cell, _placedParent, CharacterRole.Defense);
                _repo.Add(entity, cell);

                // We placed the real unit, remove the selection model.
                Object.Destroy(_selected.gameObject);
            }
            else
            {
                _selected.ResetPosition();
            }

            _selected = null;
        }

        private void CancelDrag()
        {
            _dragging = false;
            if (_selected != null)
                _selected.ResetPosition();
            _selected = null;
        }
    }
}
