using UnityEngine;

using _Game.Core;                        // GameContext
using _Game.Core.DI;                     // BaseInstaller
using _Game.Interfaces;                  // IDIContainer, ISystemRunner, IEventBus

// Board / grid / input
using _Game.Runtime.Board;               // BoardSurface, BoardGrid, GridProjector, ScreenSpaceRayProvider

// Optional hover & visuals
using _Game.Runtime.Systems;             // PointerHoverSystem
using _Game.Runtime.Visuals;             // GridVisualsService

// Characters
using _Game.Runtime.Characters;          // CharacterPoolRegistry, CharacterFactory, CharacterRepository
using _Game.Runtime.Core;                // CharacterSystem

// Placement
using _Game.Runtime.Placement;           // PlacementValidator, PlacementPreviewService

// Level data + selection
using _Game.Runtime.Levels;              // LevelRuntimeConfig / LevelCatalogue
using _Game.Runtime.Selection;           // CharacterSelectionSpawner, SelectableCharacterView

// Pool util (for grid visuals)
using _Game.Utils;                       // GameObjectPool

namespace _Game.Core.DI
{
    /// <summary>
    /// Scene-level installer: grid + visuals + characters + selection.
    /// Matches the CharacterSelectionSystem ctor that takes BoardSurface and IEventBus.
    /// </summary>
    public sealed class RuntimeInstaller : BaseInstaller
    {
        // --------- Scene references ---------
        [Header("Scene")] [SerializeField] private BoardSurface boardSurface;
        [SerializeField] private Camera targetCamera;

        [Header("Selection Models")]
        [SerializeField] private Transform characterSelectionSpawnPoint;
        [SerializeField, Min(0.1f)] private float characterSpacing = 2f;
        [SerializeField] private Transform selectionParent;   // parent for spawned selection models
        [SerializeField, Min(0.05f)] private float selectionPickRadius = 1.0f; // world-units pick radius

        [Header("Placed Units Parent")]
        [SerializeField] private Transform unitsParent;

        // --------- Visuals (optional) ---------
        [Header("Grid Visuals (Optional)")]
        [SerializeField] private GameObject placeableCellPrefab;
        [SerializeField] private GameObject hoverHighlightPrefab;
        [SerializeField, Min(0f)] private float surfaceLift = 0.01f;
        [SerializeField] private string visualsRootName = "GridVisuals";
        [SerializeField] private Transform visualsParent;

        // --------- Level data ---------
        [Header("Level")]
        [SerializeField] private LevelCatalogue levelCatalogue;
        [SerializeField] private string levelId = "Level-1";

        public override void Install(IDIContainer container)
        {
            var events  = GameContext.Events;
            var systems = GameContext.Systems;

            // ---- Guards ----
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
            if (!levelCatalogue)
            {
                Debug.LogError("[RuntimeInstaller] LevelCatalogue is not assigned.");
                return;
            }

            // ==== Core runtime services =====================================================
            var rayProvider = new ScreenSpaceRayProvider(targetCamera);
            container.BindSingleton<IRayProvider>(rayProvider);

            var grid = new BoardGrid(boardSurface.rows, boardSurface.cols, boardSurface.cellSize);
            container.BindSingleton(grid);

            var projector = new GridProjector(grid, boardSurface);   // TryWorldToCell + CellToWorldCenter
            container.BindSingleton(projector);

            container.BindSingleton(boardSurface);

            // ==== Optional: pointer hover → events (hover highlight visuals) =================
            if (hoverHighlightPrefab || placeableCellPrefab)
            {
                var hoverSystem = new PointerHoverSystem(rayProvider, boardSurface, projector, events);
                systems.Register((IUpdatableSystem)hoverSystem);
            }

            // ==== Grid visuals (pooled) =====================================================
            if (placeableCellPrefab || hoverHighlightPrefab)
            {
                Transform visualsRoot = visualsParent;
                if (!visualsRoot)
                {
                    var go = new GameObject(string.IsNullOrWhiteSpace(visualsRootName) ? "GridVisuals" : visualsRootName);
                    go.transform.SetParent(boardSurface.transform, true);
                    visualsRoot = go.transform;
                }

                GameObjectPool placeablePool = null;
                if (placeableCellPrefab)
                {
                    int approxPlaceable = Mathf.CeilToInt(grid.Size.Rows * grid.Size.Cols * 0.5f);
                    placeablePool = new GameObjectPool(placeableCellPrefab, approxPlaceable, visualsRoot);
                }

                GameObject hoverGO = null;
                if (hoverHighlightPrefab)
                {
                    hoverGO = Object.Instantiate(hoverHighlightPrefab, visualsRoot, false);
                    hoverGO.name = "HoverHighlight";
                    hoverGO.SetActive(false);
                }

                var visualsSvc = new GridVisualsService(
                    grid, boardSurface, projector, events, placeablePool, hoverGO, visualsRoot, surfaceLift);
                container.BindSingleton(visualsSvc);
            }

            // ==== Level runtime config ======================================================
            var levelData = levelCatalogue.GetById(levelId);
            if (levelData == null)
            {
                Debug.LogError($"[RuntimeInstaller] Level '{levelId}' not found in LevelCatalogue.");
                return;
            }
            var level = new LevelRuntimeConfig(levelData);
            container.BindSingleton(level);

            // ==== Parents (units / selection) ===============================================
            if (!unitsParent)
            {
                var up = new GameObject("Units").transform;
                up.SetParent(boardSurface.transform, true);
                unitsParent = up;
            }
            if (!characterSelectionSpawnPoint)
            {
                var sp = new GameObject("CharacterSelectionSpawnPoint").transform;
                sp.SetParent(boardSurface.transform, true);
                sp.position = boardSurface.transform.position;
                characterSelectionSpawnPoint = sp;
            }
            if (!selectionParent)
            {
                var sel = new GameObject("SelectionRoot").transform;
                sel.SetParent(boardSurface.transform, true);
                selectionParent = sel;
            }

            // ==== Characters stack ==========================================================
            var pools      = new CharacterPoolRegistry();
            var repo       = new CharacterRepository();
            var factory    = new CharacterFactory(pools);
            var charSystem = new CharacterSystem();

            container.BindSingleton(pools);
            container.BindSingleton(repo);
            container.BindSingleton(factory);
            container.BindSingleton(charSystem);

            systems.Register((IUpdatableSystem)charSystem);

            // ==== Placement validator (bounds + occupancy) ==================================
            var validator  = new PlacementValidator(repo, grid);
            container.BindSingleton(validator);

            // ==== Placement preview service =================================================
            var previewSvc = new PlacementPreviewService(factory, boardSurface, projector, validator, unitsParent);
            container.BindSingleton(previewSvc);

            // ==== Spawn 3D selection models (no physics) ====================================
            var spawner     = new CharacterSelectionSpawner(level, characterSelectionSpawnPoint, characterSpacing, selectionParent);
            var selectables = spawner.Spawn(); // List<SelectableCharacterView>

            // ==== Register selection + placement system =====================================
            var selectionSystem = new CharacterSelectionSystem(
                rayProvider,
                projector,
                grid,
                boardSurface,
                factory,
                repo,
                validator,
                unitsParent,
                selectables,
                GameContext.Events,
                dragLift: 0.01f
            );
            systems.Register((IUpdatableSystem)selectionSystem);
            
            var spawnerSystem = new _Game.Runtime.Combat.EnemySpawnerSystem(
                grid,
                projector,
                factory,
                repo,
                level,
                unitsParent
            );
            systems.Register((_Game.Interfaces.IUpdatableSystem)spawnerSystem);
        }
    }
}