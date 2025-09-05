using _Game.Core.Events;
using _Game.Enums;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Core;
using UnityEngine;

namespace _Game.Runtime.Placement
{
    public class PlacementControllerSystem : IUpdatableSystem
    {
        private readonly IEventBus _eventBus;
        private readonly BoardGrid _grid;
        private readonly CharacterFactory _factory;
        private readonly CharacterRepository _repo;
        private readonly PlacementValidator _validator;
        private readonly PlacementPreviewService _preview;
        private readonly Transform _parent;

        private Cell? _hoveredCell;
        private bool _isDragging;

        public PlacementControllerSystem(
            IEventBus eventBus,
            BoardGrid grid,
            CharacterFactory factory,
            CharacterRepository repo,
            PlacementValidator validator,
            PlacementPreviewService preview,
            Transform parent)
        {
            _eventBus = eventBus;
            _grid = grid;
            _factory = factory;
            _repo = repo;
            _validator = validator;
            _preview = preview;
            _parent = parent;

            _eventBus.Subscribe<CharacterSelectedEvent>(OnCharacterSelected);
            _eventBus.Subscribe<HoverCellChangedEvent>(OnHoverCellChanged);
        }

        public void Tick()
        {
            if (_preview.CurrentArchetype == null)
                return;

            // Begin drag
            if (!_isDragging && Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _eventBus.Fire(new PlacementModeChangedEvent(true));
            }

            // Drag update
            if (_isDragging && _hoveredCell.HasValue)
            {
                _preview.UpdateTo(_hoveredCell);
            }

            // Drop
            if (_isDragging && Input.GetMouseButtonUp(0))
            {
                _isDragging = false;

                if (_hoveredCell.HasValue && _validator.IsValid(_hoveredCell.Value))
                {
                    var cell = _hoveredCell.Value;
                    var entity = _factory.Spawn(_preview.CurrentArchetype, cell, _parent, CharacterRole.Defense);
                    _repo.Add(entity, cell);

                    _eventBus.Fire(new CharacterPlacedEvent(_preview.CurrentArchetype, entity.EntityId, cell));
                    _preview.End();
                    _eventBus.Fire(new PlacementModeChangedEvent(false));
                }
                else
                {
                    _preview.SnapBackToOrigin();
                    _eventBus.Fire(new PlacementModeChangedEvent(false));
                }
            }
        }

        private void OnCharacterSelected(CharacterSelectedEvent e)
        {
            _preview.Begin(e.Archetype);
        }

        private void OnHoverCellChanged(HoverCellChangedEvent e)
        {
            _hoveredCell = e.Cell;
        }
    }
}