using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Characters.Config;
using _Game.Runtime.Characters.View;
using _Game.Runtime.Core;
using UnityEngine;

namespace _Game.Runtime.Placement
{
    public class PlacementPreviewService
    {
        private readonly CharacterFactory _factory;
        private readonly BoardSurface _surface;
        private readonly GridProjector _projector;
        private readonly PlacementValidator _validator;
        private readonly Transform _parent;

        private GameObject _currentGhost;
        private CharacterView _ghostView;
        private CharacterArchetype _archetype;
        private Vector3 _spawnOrigin;

        public PlacementPreviewService(
            CharacterFactory factory,
            BoardSurface surface,
            GridProjector projector,
            PlacementValidator validator,
            Transform parent)
        {
            _factory = factory;
            _surface = surface;
            _projector = projector;
            _validator = validator;
            _parent = parent;
        }

        public void Begin(CharacterArchetype archetype)
        {
            _archetype = archetype;
            _currentGhost = _factory.GetPreview(archetype, _parent);
            _ghostView = _currentGhost.GetComponent<CharacterView>();
            _spawnOrigin = _currentGhost.transform.position;
        }

        public void UpdateTo(Cell? cell)
        {
            if (_currentGhost == null || !cell.HasValue) return;

            var worldPos = _projector.CellToWorldCenter(cell.Value);
            _currentGhost.transform.position = worldPos;

            bool valid = _validator.IsValid(cell.Value);
            _ghostView?.SetGhostVisual(true, valid);
        }

        public void SnapBackToOrigin()
        {
            if (_currentGhost != null)
                _currentGhost.transform.position = _spawnOrigin;
        }

        public void End()
        {
            if (_currentGhost)
            {
                Object.Destroy(_currentGhost);
                _currentGhost = null;
                _ghostView = null;
            }
        }

        public CharacterArchetype CurrentArchetype => _archetype;
        public GameObject CurrentGhost => _currentGhost;
    }
}