using _Game.Core.Events;
using UnityEngine;
using _Game.Enums;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Characters.Config;
using _Game.Runtime.Core;

namespace _Game.Runtime.Placement
{
    /// <summary>
    /// Drives placement mode (UI → preview → place).
    /// - Listens to CharacterSelectedEvent to enter placement mode
    /// - Tracks HoverCellChangedEvent to know the "activated" cell
    /// - Calls preview.UpdateTo(cell) every Tick
    /// - On LMB release: attempts to place on active cell
    /// - On RMB / ESC: cancels placement
    /// </summary>
    public sealed class PlacementControllerSystem : IUpdatableSystem
    {
        private readonly IEventBus _events;
        private readonly BoardGrid _grid;
        private readonly GridProjector _projector;
        private readonly BoardSurface _surface;
        private readonly CharacterFactory _factory;
        private readonly CharacterRepository _repo;
        private readonly PlacementValidator _validator;
        private readonly PlacementPreviewService _preview;
        private readonly Transform _unitsParent;

        private CharacterArchetype _selected;
        private bool _inPlacement;
        private Cell? _activeCell;

        public PlacementControllerSystem(
            IEventBus events,
            BoardGrid grid,
            GridProjector projector,
            BoardSurface surface,
            CharacterFactory factory,
            CharacterRepository repo,
            PlacementValidator validator,
            PlacementPreviewService preview,
            Transform unitsParent)
        {
            _events       = events;
            _grid         = grid;
            _projector    = projector;
            _surface      = surface;
            _factory      = factory;
            _repo         = repo;
            _validator    = validator;
            _preview      = preview;
            _unitsParent  = unitsParent;

            _events.Subscribe<CharacterSelectedEvent>(OnCharacterSelected);
            _events.Subscribe<HoverCellChangedEvent>(OnHoverCellChanged);
        }

        public void Tick()
        {
            if (!_inPlacement) return;

            // Keep the ghost snapped to the latest hovered cell
            _preview.UpdateTo(_activeCell);

            // Place on LMB release
            if (Input.GetMouseButtonUp(0))
            {
                TryPlace();
            }

            // Cancel on RMB or ESC
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }

        private void OnCharacterSelected(CharacterSelectedEvent e)
        {
            _selected    = e.Archetype;
            _inPlacement = true;
            _preview.Begin(_selected);

            // optional: broadcast mode change if your UI needs it
            _events.Fire(new PlacementModeChangedEvent { IsActive = true });
        }

        private void OnHoverCellChanged(HoverCellChangedEvent e)
        {
            _activeCell = e.Cell; // may be null
        }

        private void TryPlace()
        {
            if (!_activeCell.HasValue)
                return;

            var cell = _activeCell.Value;
            if (!_validator.IsValid(cell))
                return;

            var center = _projector.CellToWorldCenter(cell);
            var entity = _factory.SpawnAtWorld(_selected, center, cell, _unitsParent, CharacterRole.Defense);
            _repo.Add(entity, cell);

            // Finish placement (single-place flow). If you want multi-place, remove the End() and _inPlacement = false
            _preview.End();
            _inPlacement = false;
            _events.Fire(new PlacementModeChangedEvent { IsActive = false });
        }

        private void CancelPlacement()
        {
            _preview.End();
            _inPlacement = false;
            _events.Fire(new PlacementModeChangedEvent { IsActive = false });
        }
    }
}
