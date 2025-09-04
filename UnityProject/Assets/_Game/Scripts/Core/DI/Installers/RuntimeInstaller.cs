using UnityEngine;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Core;
using _Game.Runtime.Systems;
using _Game.Runtime.Visuals;
using _Game.Core.DI;
using _Game.Utils;

namespace _Game.Core.DI
{
    public sealed class RuntimeInstaller : BaseInstaller
    {
        [Header("Scene References")]
        [SerializeField] private BoardSurface boardSurface;
        [SerializeField] private Camera targetCamera;

        [Header("Grid Visuals")]
        [Tooltip("Prefab with a SpriteRenderer (static marker for each placeable cell).")]
        [SerializeField] private GameObject placeableCellPrefab;

        [Tooltip("Prefab with a SpriteRenderer (single moving highlight).")]
        [SerializeField] private GameObject hoverHighlightPrefab;

        [Tooltip("Lift sprites slightly off the surface to avoid z-fighting.")]
        [SerializeField, Min(0f)] private float surfaceLift = 0.01f;

        [Tooltip("Optional parent name for all grid visuals.")]
        [SerializeField] private string visualsRootName = "GridVisuals";

        public override void Install(IDIContainer container)
        {
            // --- Sanity checks ---
            if (boardSurface == null)
            {
                Debug.LogError("[RuntimeInstaller] BoardSurface is not assigned.");
                return;
            }

            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("[RuntimeInstaller] Target Camera is null (and no MainCamera found).");
                return;
            }

            if (placeableCellPrefab == null)
            {
                Debug.LogError("[RuntimeInstaller] PlaceableCellPrefab is not assigned.");
                return;
            }

            if (hoverHighlightPrefab == null)
            {
                Debug.LogError("[RuntimeInstaller] HoverHighlightPrefab is not assigned.");
                return;
            }

            // --- Core runtime bindings ---
            var rayProvider = new ScreenSpaceRayProvider(targetCamera);
            container.BindSingleton<IRayProvider>(rayProvider);

            var grid = new BoardGrid(boardSurface.rows, boardSurface.cols, boardSurface.cellSize);
            container.BindSingleton(grid);

            var projector = new GridProjector(grid, boardSurface);
            container.BindSingleton(projector);
            container.BindSingleton(boardSurface);

            // --- Systems ---
            var systems = _Game.Core.GameContext.Systems;
            var events  = _Game.Core.GameContext.Events;

            var hoverSystem = new PointerHoverSystem(rayProvider, boardSurface, projector, events);
            systems.Register((IUpdatableSystem)hoverSystem);

            // --- Visuals (pooled) ---
            var visualsRootGO = new GameObject(string.IsNullOrWhiteSpace(visualsRootName) ? "GridVisuals" : visualsRootName);
            visualsRootGO.transform.SetPositionAndRotation(boardSurface.transform.position, Quaternion.identity);
            visualsRootGO.transform.SetParent(boardSurface.transform, worldPositionStays: true);

            // Pre-warm to number of placeable cells (≈ half the grid)
            int approxPlaceable = Mathf.CeilToInt((grid.Size.Rows * grid.Size.Cols) * 0.5f);
            var placeablePool = new GameObjectPool(placeableCellPrefab, approxPlaceable, visualsRootGO.transform);

            // Single highlight instance
            var hoverGO = Object.Instantiate(hoverHighlightPrefab, visualsRootGO.transform, worldPositionStays: false);
            hoverGO.name = "HoverHighlight";
            hoverGO.SetActive(false);

            // Create & bind the visuals service so others could resolve/Dispose if needed
            var visualsSvc = new GridVisualsService(grid, boardSurface, projector, events, placeablePool, hoverGO, visualsRootGO.transform, surfaceLift);
            container.BindSingleton(visualsSvc);

            Debug.Log("[RuntimeInstaller] Runtime systems + visuals installed.");
        }
    }
}
