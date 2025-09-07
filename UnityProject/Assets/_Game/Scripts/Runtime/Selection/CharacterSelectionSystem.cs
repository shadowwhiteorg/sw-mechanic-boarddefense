// Assets/_Game/Scripts/Runtime/Selection/CharacterSelectionSystem.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using _Game.Interfaces;
using _Game.Core.Events;
using _Game.Enums;
using _Game.Runtime.Core;
using _Game.Runtime.Board;
using _Game.Runtime.Placement;
using _Game.Runtime.Characters;
using _Game.Runtime.Characters.Config;
using _Game.Runtime.Levels;

namespace _Game.Runtime.Selection
{
    /// <summary>
    /// Drag-to-place system for selection row:
    /// - Click a selectable on the lineup (renderer-bounds ray test).
    /// - Drag over board plane with a small lift; on release, snap to cell if valid.
    /// - On successful placement, spawn the real defense entity and:
    ///   * auto-refill the same archetype at the same slot if level has remaining stock
    ///   * decrement the level stock by one (for the refill)
    /// - Ensures new (refilled) selectables are added to the internal list so they are pickable.
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
        private readonly List<SelectableCharacterView> _selectables;
        private readonly IEventBus _events;
        private readonly float _dragLift;

        // New: level counts + spawner for refills
        private readonly LevelRuntimeConfig _level;
        private readonly CharacterSelectionSpawner _spawner;

        private SelectableCharacterView _selected;
        private bool _dragging;
        private Vector3 _cursorWorld;
        private Cell? _hoverCell;

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
            CharacterSelectionSpawner spawner,
            LevelRuntimeConfig level,
            float dragLift = 0.01f)
        {
            _rayProvider   = rayProvider;
            _projector     = projector;
            _grid          = grid;
            _surface       = surface;
            _factory       = factory;
            _repo          = repo;
            _validator     = validator;
            _placedParent  = placedParent;
            _selectables   = selectables ?? new List<SelectableCharacterView>();
            _events        = events;
            _spawner       = spawner;
            _level         = level;
            _dragLift      = Mathf.Max(0f, dragLift); // ensure positive lift

            _events.Subscribe<HoverCellChangedEvent>(e => _hoverCell = e.Cell);
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

                    // lift to avoid z-fighting while dragging
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

        public void RegisterSelectable(SelectableCharacterView view)
        {
            if (view != null && !_selectables.Contains(view))
                _selectables.Add(view);
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
            // Use the actively hovered cell if provided by hover system; otherwise compute from cursor
            if (_hoverCell.HasValue && _validator.IsValid(_hoverCell.Value))
            {
                PlaceAndFinalize(_hoverCell.Value);
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

            var worldCenter = _projector.CellToWorldCenter(cell);
            var archetype   = _selected.Archetype;
            var refillPos   = _selected.InitialPosition;

            // Spawn the real defense entity (factory typically registers it with systems/repo)
            var entity = _factory.SpawnAtWorld(
                archetype,
                worldCenter,
                cell,
                _placedParent,
                CharacterRole.Defense);

            // If your factory doesn't auto-register, uncomment:
            // _repo.Add(entity, cell);

            // Notify others (UI, analytics, SFX) that a defense was placed
            _events?.Fire(new CharacterPlacedEvent(archetype, entity.EntityId, cell));

            // Remove the consumed selectable
            _selectables.Remove(_selected);
            UnityEngine.Object.Destroy(_selected.gameObject);
            _selected = null;

            // === Auto-refill logic ===
            if (_level != null && _spawner != null)
            {
                int remaining = _level.GetDefenseRemaining(archetype);
                if (remaining > 1)
                {
                    // Decrement once and spawn a new selectable in the same slot
                    _level.ConsumeDefenseOne(archetype);

                    var refill = _spawner.SpawnAt(archetype, refillPos);
                    RegisterSelectable(refill);
                }
                // else: no more stock, slot stays empty
            }
        }

        private void CancelDrag()
        {
            _dragging = false;
            if (_selected != null)
            {
                _selected.ResetPosition(); // back to its slot
                _selected = null;
            }
        }
    }
}
