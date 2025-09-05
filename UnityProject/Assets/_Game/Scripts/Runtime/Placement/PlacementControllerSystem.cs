using UnityEngine;
using _Game.Interfaces;
using _Game.Core.Events;
using _Game.Enums;
using _Game.Runtime.Core;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;

namespace _Game.Runtime.Placement
{
    /// <summary>
    /// Orchestrates defense placement:
    ///  - listens CharacterSelectedEvent (enter mode + spawn preview)
    ///  - listens HoverCellChangedEvent (move preview)
    ///  - LMB to place, RMB/Esc to cancel
    /// </summary>
    public sealed class PlacementControllerSystem : IUpdatableSystem
    {
        private readonly IEventBus _events;
        private readonly BoardGrid _grid;
        private readonly CharacterFactory _factory;
        private readonly CharacterRepository _repo;
        private readonly PlacementValidator _validator;
        private readonly PlacementPreviewService _preview;
        private readonly Transform _unitsParent;

        private Cell? _hoverCell;

        public PlacementControllerSystem(
            IEventBus events,
            BoardGrid grid,
            CharacterFactory factory,
            CharacterRepository repo,
            PlacementValidator validator,
            PlacementPreviewService preview,
            Transform unitsParent)
        {
            _events = events; _grid = grid; _factory = factory; _repo = repo;
            _validator = validator; _preview = preview; _unitsParent = unitsParent;

            _events.Subscribe<CharacterSelectedEvent>(OnCharacterSelected);
            _events.Subscribe<HoverCellChangedEvent>(e => { _hoverCell = e.Cell; _preview.UpdateTo(_hoverCell); });
        }

        private void OnCharacterSelected(CharacterSelectedEvent e)
        {
            // (Re)start placement mode for the selected archetype.
            _preview.Begin(e.Archetype);
            _events.Fire(new PlacementModeChangedEvent(true));
            _preview.UpdateTo(_hoverCell);
        }

        public void Tick()
        {
            // Cancel
            if (_preview.IsActive && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
                CancelPlacement();

            // Confirm
            if (_preview.IsActive && Input.GetMouseButtonDown(0))
                TryPlace();
        }

        private void TryPlace()
        {
            if (!_hoverCell.HasValue) return;
            var cell = _hoverCell.Value;
            if (!_validator.IsValid(cell)) return;

            // Occupy grid and spawn real unit (pooled)
            _grid.TryOccupy(cell);
            var archetype = _preview.CurrentArchetype;
            var entity = _factory.Spawn(archetype, cell, _unitsParent, CharacterRole.Defense);
            _repo.Add(entity, cell);

            _events.Fire(new CharacterPlacedEvent(archetype, entity.EntityId, cell));
            EndPlacement();
        }

        private void CancelPlacement() => EndPlacement();

        private void EndPlacement()
        {
            _preview.End();
            _events.Fire(new PlacementModeChangedEvent(false));
        }
    }
}
