using UnityEngine;
using _Game.Runtime.Board;
using _Game.Runtime.Characters;
using _Game.Runtime.Characters.Config;
using _Game.Runtime.Core;

namespace _Game.Runtime.Placement
{
    public sealed class PlacementPreviewService
    {
        private readonly BoardSurface _surface;
        private readonly GridProjector _projector;
        private readonly PlacementValidator _validator;
        private readonly Transform _parent;
        private readonly float _lift;

        private GameObject _ghost;
        private Renderer[] _ghostRenderers;
        private CharacterArchetype _current;
        private bool _active;

        private static readonly Color kValid   = new Color(0.35f, 1.00f, 0.35f, 0.55f);
        private static readonly Color kInvalid = new Color(1.00f, 0.35f, 0.35f, 0.55f);

        public PlacementPreviewService(
            CharacterFactory factory,     
            BoardSurface surface,
            GridProjector projector,
            PlacementValidator validator,
            Transform parent,
            float lift = 0.01f)
        {
            _surface   = surface;
            _projector = projector;
            _validator = validator;
            _parent    = parent;
            _lift      = Mathf.Max(0f, lift);
        }

        public CharacterArchetype CurrentArchetype => _current;


        public void End()
        {
            _active = false;
            _current = null;

            if (_ghost)
            {
                UnityEngine.Object.Destroy(_ghost);
                _ghost = null;
                _ghostRenderers = null;
            }
        }
    }
}
