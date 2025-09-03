using UnityEngine;
using _Game.Runtime.Core;
using _Game.Runtime.Board;

namespace _Game.Runtime.Debugging
{
    /// <summary>
    /// Lightweight play-mode HUD to visualize the cell under the pointer.
    /// Not for shipping; helps validate math without physics.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BoardDebugHUD : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private BoardSurface boardSurface;
        [SerializeField] private bool drawMarker = true;

        private BoardGrid _grid;
        private GridProjector _projector;
        private IRayProvider _rayProvider;

        private Cell? _hoverCell;
        private Vector3 _hoverWorld;

        private void Awake()
        {
            if (!targetCamera) targetCamera = Camera.main;
            _rayProvider = new ScreenSpaceRayProvider(targetCamera);
            _grid = new BoardGrid(boardSurface.rows, boardSurface.cols, boardSurface.cellSize);
            _projector = new GridProjector(_grid, boardSurface);
        }

        private void Update()
        {
            var ray = _rayProvider.PointerToRay(Input.mousePosition);
            if (InputProjectionMath.TryRayPlane(ray, boardSurface.WorldPlanePoint, boardSurface.WorldPlaneNormal, out var hit))
            {
                _hoverWorld = hit;
                _hoverCell = _projector.TryWorldToCell(hit, out var c) ? c : (Cell?)null;
            }
            else _hoverCell = null;
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
            if (!Application.isPlaying || !drawMarker || !_hoverCell.HasValue) return;
            Gizmos.color = Color.yellow;
            var p = _projector.CellToWorldCenter(_hoverCell.Value);
            Gizmos.DrawSphere(p + boardSurface.WorldPlaneNormal * 0.02f, 0.05f);
        }
    }
}
