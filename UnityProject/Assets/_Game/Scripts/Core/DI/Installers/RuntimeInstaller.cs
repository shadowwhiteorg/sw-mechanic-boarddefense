// Assets/_Game/Scripts/_Core/DI/RuntimeInstaller.cs
using UnityEngine;

using _Game.Core;                        // GameContext
using _Game.Core.DI;                     // BaseInstaller
using _Game.Interfaces;                  // IDIContainer, IUpdatableSystem, IEventBus

// Board
using _Game.Runtime.Board;               // BoardSurface, BoardGrid, GridProjector, ScreenSpaceRayProvider

// Characters
using _Game.Runtime.Characters;          // CharacterPoolRegistry, CharacterFactory, CharacterRepository, CharacterSystem

// Placement & Selection (DEFENSE flow)
using _Game.Runtime.Placement;           // PlacementValidator, PlacementPreviewService
using _Game.Runtime.Selection;           // CharacterSelectionSpawner, CharacterSelectionSystem

// Level
using _Game.Runtime.Levels;              // LevelCatalogue, LevelRuntimeConfig

// Combat
using _Game.Runtime.Combat;              // ProjectileSystem, BaseHealthSystem, CharacterLifetimeSystem, GameStateSystem
using _Game.Runtime.Core;                // EnemySpawnerSystem

// Visuals
using _Game.Runtime.Systems;             // PointerHoverSystem
using _Game.Runtime.Visuals;             // GridVisualsService

// Utils
using _Game.Utils;                       // GameObjectPool

namespace _Game.Core.DI
{
    /// <summary>
    /// Scene-level wiring for board/grid, characters, projectiles, enemy spawns,
    /// defense selection/placement, grid visuals, and simple WIN/LOSE logs.
    /// </summary>
    public sealed class RuntimeInstaller : BaseInstaller
    {
        [Header("Scene")]
        [SerializeField] private BoardSurface boardSurface;
        [SerializeField] private Camera targetCamera;

        [Header("Parents")]
        [SerializeField] private Transform unitsParent;        // spawned defenses & enemies
        [SerializeField] private Transform projectilesParent;  // pooled projectiles

        [Header("Grid Visuals (Prefabs)")]
        [SerializeField] private GameObject placeableCellPrefab;
        [SerializeField] private GameObject hoverHighlightPrefab;
        [SerializeField] private float     surfaceLift = 0.01f;
        [SerializeField] private string    visualsRootName = "GridVisuals";
        [SerializeField] private Transform visualsParent;

        [Header("Level")]
        [SerializeField] private LevelCatalogue levelCatalogue;
        [SerializeField] private string levelId = "Level-1";

        [Header("Projectiles (optional prefab)")]
        [SerializeField] private GameObject projectilePrefab;

        [Header("Selection / Placement")]
        [SerializeField] private Transform selectionModelsParent;
        [SerializeField] private Transform selectionSpawnPoint;
        [SerializeField, Min(0.25f)] private float selectionSpacing   = 2f;
        [SerializeField, Min(0.1f)]  private float selectionPickRadius = 1.0f;

        public override void Install(IDIContainer container)
        {
            var events  = GameContext.Events;
            var systems = GameContext.Systems;

            // ---- Guards ----
            if (!boardSurface) { Debug.LogError("[RuntimeInstaller] BoardSurface is not assigned."); return; }
            if (!targetCamera) targetCamera = Camera.main;
            if (!targetCamera) { Debug.LogError("[RuntimeInstaller] Target Camera is null (and no MainCamera found)."); return; }
            if (!levelCatalogue) { Debug.LogError("[RuntimeInstaller] LevelCatalogue is not assigned."); return; }

            // ==== Core: grid & projection ====
            var rayProvider = new ScreenSpaceRayProvider(targetCamera);
            container.BindSingleton<IRayProvider>(rayProvider);

            var grid      = new BoardGrid(boardSurface.rows, boardSurface.cols, boardSurface.cellSize);
            var projector = new GridProjector(grid, boardSurface);

            container.BindSingleton(boardSurface);
            container.BindSingleton(grid);
            container.BindSingleton(projector);

            // ==== Grid visuals (prefab-driven) ====
            var hoverSystem = new PointerHoverSystem(rayProvider, boardSurface, projector, events);
            systems.Register((IUpdatableSystem)hoverSystem);

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
                    placeablePool = new GameObjectPool(placeableCellPrefab, initialSize: approxPlaceable, parent: visualsRoot);
                }

                GameObject hoverGO = null;
                if (hoverHighlightPrefab)
                {
                    hoverGO = Object.Instantiate(hoverHighlightPrefab, visualsRoot, false);
                    hoverGO.name = "HoverHighlight";
                    hoverGO.SetActive(false);
                }

                var visualsSvc = new GridVisualsService(
                    grid, boardSurface, projector, events,
                    placeablePool, hoverGO, visualsRoot, surfaceLift);
                container.BindSingleton(visualsSvc);
            }

            // ==== Level runtime ====
            var levelData = levelCatalogue.GetById(levelId);
            if (levelData == null) { Debug.LogError($"[RuntimeInstaller] Level '{levelId}' not found in LevelCatalogue."); return; }
            var level = new LevelRuntimeConfig(levelData);
            container.BindSingleton(level);

            // ==== Parents ====
            if (!unitsParent)
            {
                unitsParent = new GameObject("Units").transform;
                unitsParent.SetParent(boardSurface.transform, true);
            }
            if (!projectilesParent)
            {
                projectilesParent = new GameObject("Projectiles").transform;
                projectilesParent.SetParent(boardSurface.transform, true);
            }
            if (!selectionModelsParent)
            {
                selectionModelsParent = new GameObject("SelectionModels").transform;
                selectionModelsParent.SetParent(boardSurface.transform, true);
            }
            if (!selectionSpawnPoint)
            {
                selectionSpawnPoint = new GameObject("SelectionSpawnPoint").transform;
                selectionSpawnPoint.SetParent(boardSurface.transform, true);
                selectionSpawnPoint.position = boardSurface.transform.position;
            }

            // ==== Characters stack ====
            var pools      = new CharacterPoolRegistry();
            var repo       = new CharacterRepository();
            var factory    = new CharacterFactory(pools);
            var charSystem = new CharacterSystem();

            container.BindSingleton(pools);
            container.BindSingleton(repo);
            container.BindSingleton(factory);
            container.BindSingleton(charSystem);
            systems.Register((IUpdatableSystem)charSystem);

            // ==== Lifetime + Base HP + Win/Lose ====
            var lifetime   = new CharacterLifetimeSystem(repo, events);
            systems.Register((IUpdatableSystem)lifetime);

            var baseHealth = new BaseHealthSystem(events, repo, maxHp: 1);
            container.BindSingleton(baseHealth);
            systems.Register((IUpdatableSystem)baseHealth);

            var gameState  = new GameStateSystem(events, repo, level, baseHealth);
            container.BindSingleton(gameState);
            systems.Register((IUpdatableSystem)gameState);

            // ==== Projectile pool + system ====
            if (projectilePrefab == null)
            {
                projectilePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projectilePrefab.name = "Projectile";
                projectilePrefab.transform.localScale = Vector3.one * 0.1f;
                var col = projectilePrefab.GetComponent<Collider>();
                if (col) Object.Destroy(col);
            }
            var projectilePool   = new GameObjectPool(projectilePrefab, initialSize: 32, parent: projectilesParent);
            var projectileSystem = new ProjectileSystem(events, repo, projectilePool);
            systems.Register((IUpdatableSystem)projectileSystem);

            // ==== Enemy spawner (top row, random column) ====
            var spawner = new EnemySpawnerSystem(grid, projector, factory, repo, level, unitsParent);
            systems.Register((IUpdatableSystem)spawner);

            // ==== DEFENSE selection + placement ====
            var validator = new PlacementValidator(repo, grid);
            container.BindSingleton(validator);

            var previewSvc = new PlacementPreviewService(factory, boardSurface, projector, validator, unitsParent);
            container.BindSingleton(previewSvc);

            var selectionSpawner = new CharacterSelectionSpawner(level, selectionSpawnPoint, selectionSpacing, selectionModelsParent);
            var selectables = selectionSpawner.Spawn();

            var selectionSystem = new CharacterSelectionSystem(
                rayProvider, projector, grid, boardSurface,
                factory, repo, validator, unitsParent,
                selectables, events, dragLift: 0.01f);
            systems.Register((IUpdatableSystem)selectionSystem);
        }
    }
}
