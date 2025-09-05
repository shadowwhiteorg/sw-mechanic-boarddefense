using UnityEngine;
using System.Collections.Generic;
using _Game.Interfaces;

namespace _Game.Runtime.Selection
{
    public class CharacterSelectionSystem : IUpdatableSystem
    {
        private readonly Camera _camera;
        private readonly List<SelectableCharacterView> _views;

        private SelectableCharacterView _hovered;

        public CharacterSelectionSystem(Camera camera, List<SelectableCharacterView> views)
        {
            _camera = camera;
            _views = views;
        }

        public void Tick()
        {
            Vector2 mouse = Input.mousePosition;
            SelectableCharacterView hit = null;

            foreach (var view in _views)
            {
                if (view.ContainsScreenPoint(_camera, mouse))
                {
                    hit = view;
                    break;
                }
            }

            if (_hovered != hit)
            {
                _hovered?.SetHovered(false);
                hit?.SetHovered(true);
                _hovered = hit;
            }

            if (_hovered != null && Input.GetMouseButtonDown(0))
            {
                _hovered.Click();
            }
        }
    }
}