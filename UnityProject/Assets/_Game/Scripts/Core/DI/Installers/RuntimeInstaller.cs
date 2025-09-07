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
using _Game.Runtime.Combat;              // ProjectileSystem, CharacterLifetimeSystem, GameStateSystem
using _Game.Runtime.Core;                // EnemySpawnerSystem (counts-based)

// Visuals
using _Game.Runtime.Systems;             // PointerHoverSystem
using _Game.Runtime.Visuals;             // GridVisualsService

// Utils
using _Game.Utils;                       // GameObjectPool

namespace _Game.Core.DI
{
    /// <summary>
    /// Scene-level wiring for:
    /// - Board/grid + projector + hover
    /// - Grid visuals (optional, prefab-driven)
    /// - Characters (factory/repo/system)
    /// - Enemy spawning from LevelRuntimeConfig counts (top row, random column)
    /// - Event-driven GameState (Lose on first base hit, Win when all planned enemies resolved)
    /// - Defense selection/placement with evenly spaced lineup between two border points
    /// - Projectiles (pooled)
    /// </summary>
    public sealed class RuntimeInstaller : BaseInstaller
    {
        [Header("Scene")] [SerializeField] private BoardSurface boardSurface;
        [SerializeField] private Camera targetCamera;

        [Header("Parents")] [SerializeField] private Transform unitsParent; // spawned defenses & enemies
        [SerializeField] private Transform projectilesParent; // pooled projectiles

        [Header("Grid Visuals (Prefabs)")] [SerializeField]
        private GameObject placeableCellPrefab;

        [SerializeField] private GameObject hoverHighlightPrefab;
        [SerializeField, Min(0f)] private float surfaceLift = 0.01f;
        [SerializeField] private string visualsRootName = "GridVisuals";
        [SerializeField] private Transform visualsParent;

        [Header("Level")] [SerializeField] private LevelCatalogue levelCatalogue;
        [SerializeField] private string levelId = "Level-1";

        [Header("Projectiles (optional prefab)")] [SerializeField]
        private GameObject projectilePrefab;

        [Header("Selection / Placement")] [SerializeField]
        private Transform selectionModelsParent;

        [SerializeField] private Transform selectionSpawnLeft;
        [SerializeField] private Transform selectionSpawnRight;

        [SerializeField, Min(0.0f)] private float dragLiftWhileDragging = 0.01f;

        [Header("UI - Selection Slot HUD")] [SerializeField]
        private GameObject slotHudPrefab;

        [SerializeField] private float slotHudYOffset = 0.25f;


        public override void Install(IDIContainer container)
        {
            var events = GameContext.Events;
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

            // ==== Core: grid & projection ====
            var rayProvider = new ScreenSpaceRayProvider(targetCamera);
            container.BindSingleton<IRayProvider>(rayProvider);

            var grid = new BoardGrid(boardSurface.rows, boardSurface.cols, boardSurface.cellSize);
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
                    var go = new GameObject(
                        string.IsNullOrWhiteSpace(visualsRootName) ? "GridVisuals" : visualsRootName);
                    go.transform.SetParent(boardSurface.transform, true);
                    visualsRoot = go.transform;
                }

                GameObjectPool placeablePool = null;
                if (placeableCellPrefab)
                {
                    int approxPlaceable = Mathf.CeilToInt(grid.Size.Rows * grid.Size.Cols * 0.5f);
                    placeablePool = new GameObjectPool(placeableCellPrefab, initialSize: approxPlaceable,
                        parent: visualsRoot);
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
            if (levelData == null)
            {
                Debug.LogError($"[RuntimeInstaller] Level '{levelId}' not found in LevelCatalogue.");
                return;
            }

            var level = new LevelRuntimeConfig(levelData); // must expose DefenseRemaining & EnemyRemaining dictionaries
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

            if (!selectionSpawnLeft)
            {
                selectionSpawnLeft = new GameObject("SelectionSpawnLeft").transform;
                selectionSpawnLeft.SetParent(boardSurface.transform, true);
                selectionSpawnLeft.position = boardSurface.transform.position + new Vector3(-2f, 0f, -0.5f);
            }

            if (!selectionSpawnRight)
            {
                selectionSpawnRight = new GameObject("SelectionSpawnRight").transform;
                selectionSpawnRight.SetParent(boardSurface.transform, true);
                selectionSpawnRight.position = boardSurface.transform.position + new Vector3(+2f, 0f, -0.5f);
            }

            // ==== Characters stack ====
            var pools = new CharacterPoolRegistry();
            var repo = new CharacterRepository();
            var factory = new CharacterFactory(pools);
            var charSystem = new CharacterSystem();

            container.BindSingleton(pools);
            container.BindSingleton(repo);
            container.BindSingleton(factory);
            container.BindSingleton(charSystem);
            systems.Register((IUpdatableSystem)charSystem);

            // ==== Lifetime (despawns, cleanup) ====
            var lifetime = new CharacterLifetimeSystem(repo, events);
            systems.Register((IUpdatableSystem)lifetime);

            // ==== Game state (HP-less): lose on first base hit, win when all planned enemies resolved ====
            var gameState = new GameStateSystem(events, level);
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

            var projectilePool = new GameObjectPool(projectilePrefab, initialSize: 32, parent: projectilesParent);
            var projectileSystem = new ProjectileSystem(events, repo, projectilePool);
            systems.Register((IUpdatableSystem)projectileSystem);

            // ==== Enemy spawner (counts-based; top row, random column) ====
            // This EnemySpawnerSystem takes LevelRuntimeConfig + IEventBus (fires EnemySpawnedEvent)
            var enemySpawner = new EnemySpawner(
                grid, projector, factory, level, unitsParent, events,
                spawnInterval: 1.0f, startDelay: 0.25f);
            systems.Register((IUpdatableSystem)enemySpawner);

            // ==== DEFENSE selection + placement (evenly between two points) ====
            var validator = new PlacementValidator(repo, grid);
            container.BindSingleton(validator);

            var previewSvc = new PlacementPreviewService(factory, boardSurface, projector, validator, unitsParent);
            container.BindSingleton(previewSvc);

            // Evenly spaced lineup between Left/Right
            var selectionSpawner = new CharacterSelectionSpawner(
                level, selectionSpawnLeft, selectionSpawnRight, selectionModelsParent);
            var selectables = selectionSpawner.Spawn();

            // Pass spawner + level so auto-refill can register new selectables
            var selectionSystem = new CharacterSelectionSystem(
                rayProvider, projector, grid, boardSurface,
                factory, repo, validator, unitsParent,
                selectables, events,
                spawner: selectionSpawner,
                level: level,
                dragLift: dragLiftWhileDragging);
            systems.Register((IUpdatableSystem)selectionSystem);

            if (slotHudPrefab != null)
            {
                var hudController = new SelectionHudController(level, events, targetCamera, slotHudPrefab, selectionModelsParent, slotHudYOffset);
                container.BindSingleton(hudController);

                // This reads CharacterSelectionSpawner.SlotAnchors so HUDs spawn evenly along Left<->Right
                hudController.BuildFromSpawner(selectionSpawner);
            }
        }
    }
}
