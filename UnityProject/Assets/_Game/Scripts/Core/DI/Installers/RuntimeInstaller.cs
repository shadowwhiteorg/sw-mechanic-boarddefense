using UnityEngine;
using _Game.Interfaces;                  
using _Game.Runtime.Board;               
using _Game.Runtime.Core;                
using _Game.Runtime.Systems;             
using _Game.Runtime.Visuals;             
using _Game.Runtime.Characters;          
using _Game.Runtime.Placement;           
using _Game.Runtime.Levels;
using _Game.Runtime.Selection;
using _Game.Utils;                       

namespace _Game.Core.DI
{
    public sealed class RuntimeInstaller : BaseInstaller
    {
        // --------- Scene references ---------
        [Header("Scene")]
        [SerializeField] private BoardSurface boardSurface;
        [SerializeField] private Camera       targetCamera;
        [SerializeField] private Transform characterSelectionSpawnPoint;
        [SerializeField] private float characterSpacing = 2f;

        [Header("Parents (optional)")]
        [SerializeField] private Transform visualsParent;
        [SerializeField] private Transform unitsParent;

        // --------- Visuals ---------
        [Header("Grid Visuals")]
        [Tooltip("Static marker shown on every placeable cell.")]
        [SerializeField] private GameObject placeableCellPrefab;
        [Tooltip("Single highlight that moves to hovered cell.")]
        [SerializeField] private GameObject hoverHighlightPrefab;
        [SerializeField, Min(0f)] private float surfaceLift = 0.01f;
        [SerializeField] private string visualsRootName = "GridVisuals";

        // --------- Level data ---------
        [Header("Level")]
        [SerializeField] private LevelCatalogue levelCatalogue;
        [SerializeField] private string levelId = "Level-1";

        // ======================================================================

        public override void Install(IDIContainer container)
        {
            // Resolve global services from GameInstaller
            var events  = GameContext.Events;
            var systems = GameContext.Systems;

            // ---- Sanity checks ----
            if (!boardSurface)
            {
                Debug.LogError("[RuntimeInstaller] BoardSurface is not assigned.");
                return;
            }

            if (!targetCamera) targetCamera = Camera.main;
            if (!targetCamera)
            {
                Debug.LogError("[RuntimeInstaller] Target Camera is null (and no MainCamera found).");
                return;
            }

            // ==== Core runtime bindings =====================================================

            var rayProvider = new ScreenSpaceRayProvider(targetCamera);
            container.BindSingleton<IRayProvider>(rayProvider);

            var grid = new BoardGrid(boardSurface.rows, boardSurface.cols, boardSurface.cellSize);
            container.BindSingleton(grid);

            var projector = new GridProjector(grid, boardSurface);
            container.BindSingleton(projector);

            container.BindSingleton(boardSurface);

            // ==== Pointer hover → events ====================================================

            var hoverSystem = new PointerHoverSystem(rayProvider, boardSurface, projector, events);
            systems.Register((IUpdatableSystem)hoverSystem);

            // ==== Grid visuals (pooled) =====================================================

            Transform visualsRoot = visualsParent;
            if (!visualsRoot)
            {
                var go = new GameObject(string.IsNullOrWhiteSpace(visualsRootName) ? "GridVisuals" : visualsRootName);
                go.transform.SetParent(boardSurface.transform, true);
                visualsRoot = go.transform;
            }

            if (placeableCellPrefab && hoverHighlightPrefab)
            {
                int approxPlaceable = Mathf.CeilToInt(grid.Size.Rows * grid.Size.Cols * 0.5f);
                var placeablePool = new GameObjectPool(placeableCellPrefab, approxPlaceable, visualsRoot);

                var hoverGO = Object.Instantiate(hoverHighlightPrefab, visualsRoot, false);
                hoverGO.name = "HoverHighlight";
                hoverGO.SetActive(false);

                var visualsSvc = new GridVisualsService(
                    grid, boardSurface, projector, events, placeablePool, hoverGO, visualsRoot, surfaceLift);
                container.BindSingleton(visualsSvc);
            }
            else
            {
                Debug.LogWarning("[RuntimeInstaller] Grid Visuals prefabs not set; skipping grid markers/highlight.");
            }

            // ==== Level runtime config ======================================================

            if (!levelCatalogue)
            {
                Debug.LogError("[RuntimeInstaller] LevelCatalogue is not assigned.");
                return;
            }

            var levelData = levelCatalogue.GetById(levelId);
            if (levelData == null)
            {
                Debug.LogError($"[RuntimeInstaller] Level '{levelId}' not found in LevelCatalogue.");
                return;
            }

            var level = new LevelRuntimeConfig(levelData);
            container.BindSingleton(level);

            // ==== Parents (units) ===========================================================

            if (!unitsParent)
            {
                var up = new GameObject("Units").transform;
                up.SetParent(boardSurface.transform, true);
                unitsParent = up;
            }

            // ==== Characters stack (no combat; just spawn + track + (future) tick) ==========

            var pools      = new CharacterPoolRegistry();
            var repo       = new CharacterRepository();
            var factory    = new CharacterFactory(pools /* no combat deps in this variant */);
            var charSystem = new CharacterSystem(); // harmless to register now; useful when plugins arrive

            container.BindSingleton(pools);
            container.BindSingleton(repo);
            container.BindSingleton(factory);
            container.BindSingleton(charSystem);

            systems.Register((IUpdatableSystem)charSystem);
            
            // --- Spawn character selection ---
            var selectorSpawner = new CharacterSelectionSpawner(level, characterSelectionSpawnPoint, characterSpacing, events);
            var selectorViews = selectorSpawner.Spawn();

            // --- Register non-physics selection system ---
            var selectionSystem = new CharacterSelectionSystem(targetCamera, selectorViews);
            systems.Register(selectionSystem);
            
            // ==== Placement pipeline ========================================================
    
            var validator  = new PlacementValidator(repo, grid);
            container.BindSingleton(validator);

            var previewSvc = new PlacementPreviewService(factory, boardSurface, projector, validator, unitsParent);
            container.BindSingleton(previewSvc);

            var placementSys = new PlacementControllerSystem(
                events, grid, factory, repo, validator, previewSvc, unitsParent);
            systems.Register((IUpdatableSystem)placementSys);

            Debug.Log("[RuntimeInstaller] Runtime systems (selection, hover, visuals, level, characters, placement) installed.");
        }
    }
}
