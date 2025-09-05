using UnityEngine;
using _Game.Runtime.Core;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Characters.Config;
using _Game.Runtime.Characters.View;

namespace _Game.Runtime.Placement
{
    /// <summary>
    /// Manages the ghost (preview) character during placement.
    /// </summary>
    public sealed class PlacementPreviewService
    {
        private readonly CharacterFactory _factory;
        private readonly BoardSurface _surface;
        private readonly GridProjector _projector;
        private readonly PlacementValidator _validator;
        private readonly Transform _parent;

        private CharacterArchetype _current;
        private GameObject _ghost;

        public PlacementPreviewService(CharacterFactory factory, BoardSurface surface,
                                       GridProjector projector, PlacementValidator validator,
                                       Transform parent)
        {
            _factory = factory; _surface = surface; _projector = projector;
            _validator = validator; _parent = parent;
        }

        public void Begin(CharacterArchetype archetype)
        {
            End();
            _current = archetype;
            _ghost = _factory.GetPreview(archetype, _parent);
            SetTint(false);
        }

        public void UpdateTo(Cell? cell)
        {
            if (_ghost == null) return;

            bool valid = false;
            Vector3 pos = _ghost.transform.position;

            if (cell.HasValue)
            {
                var c = cell.Value;
                valid = _validator.IsValid(c);
                pos = _projector.CellToWorldCenter(c) + _surface.WorldPlaneNormal * 0.01f;
            }

            _ghost.transform.position = pos;
            SetTint(valid);
        }

        public void End()
        {
            if (_ghost != null && _current != null)
                _factory.ReturnPreview(_current, _ghost);

            _ghost = null;
            _current = null;
        }

        private void SetTint(bool valid)
        {
            var view = _ghost ? _ghost.GetComponent<CharacterView>() : null;
            if (view) view.SetGhostVisual(true, valid);
        }

        public bool IsActive => _ghost != null;
        public CharacterArchetype CurrentArchetype => _current;
    }
}
