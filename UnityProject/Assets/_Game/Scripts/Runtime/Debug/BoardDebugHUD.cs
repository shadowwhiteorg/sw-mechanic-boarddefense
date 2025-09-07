using UnityEngine;
using _Game.Core;
using _Game.Core.Events;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Core;

namespace _Game.Runtime.Debugging
{
    [DisallowMultipleComponent]
    public sealed class BoardDebugHUD : MonoBehaviour
    {
        [SerializeField] private BoardSurface boardSurface;
        [SerializeField] private bool drawMarker = true;

        private IEventBus _events;
        private Cell? _hoverCell;
        private Vector3 _hoverWorld;

        private void OnEnable()
        {
            _events = GameContext.Events;
            if (_events != null)
                _events.Subscribe<HoverCellChangedEvent>(OnHoverChanged);
        }

        private void OnDisable()
        {
            if (_events != null)
                _events.Unsubscribe<HoverCellChangedEvent>(OnHoverChanged);
        }

        private void OnHoverChanged(HoverCellChangedEvent e)
        {
            _hoverCell  = e.Cell;
            _hoverWorld = e.World;
        }

        private void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.box) { fontSize = 14, alignment = TextAnchor.UpperLeft };
            var label = _hoverCell.HasValue
                ? $"Hover Cell: { _hoverCell.Value }  World: {_hoverWorld}"
                : "Hover Cell: (none)";
            GUI.Box(new Rect(10, 10, 520, 48), label, style);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !drawMarker || !_hoverCell.HasValue || boardSurface == null) return;
            Gizmos.color = Color.yellow;
            var p = _hoverWorld + boardSurface.WorldPlaneNormal * 0.02f;
            Gizmos.DrawSphere(p, 0.05f);
        }
    }
}