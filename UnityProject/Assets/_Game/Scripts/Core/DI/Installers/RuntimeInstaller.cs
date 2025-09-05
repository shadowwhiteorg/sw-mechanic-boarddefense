using UnityEngine;
using _Game.Interfaces;       
using _Game.Runtime.Board;    
using _Game.Runtime.Systems;  
using _Game.Runtime.Visuals;  
using _Game.Runtime.Characters;
using _Game.Runtime.Core;     
using _Game.Runtime.Placement;
using _Game.Runtime.Levels;   
using _Game.Runtime.Selection;
using _Game.Utils;            

namespace _Game.Core.DI
{

    public sealed class RuntimeInstaller : BaseInstaller
    {
        [Header("Scene")]
        [SerializeField] private BoardSurface boardSurface;
        [SerializeField] private Camera       targetCamera;

        [Header("Selection Models")]
        [SerializeField] private Transform characterSelectionSpawnPoint;
        [SerializeField, Min(0.1f)] private float characterSpacing = 2f;
        [SerializeField] private Transform selectionParent;
        [SerializeField, Min(0.05f)] private float selectionPickRadius = 1.0f;

        [Header("Placed Units Parent")]
        [SerializeField] private Transform unitsParent;

        [Header("Grid Visuals (Optional)")]
        [SerializeField] private GameObject placeableCellPrefab;
        [SerializeField] private GameObject hoverHighlightPrefab;
        [SerializeField] private float surfaceLift = 0.01f;
        [SerializeField] private string visualsRootName = "GridVisuals";
        [SerializeField] private Transform visualsParent;

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

            var rayProvider = new ScreenSpaceRayProvider(targetCamera);
            container.BindSingleton<IRayProvider>(rayProvider);

            var grid = new BoardGrid(boardSurface.rows, boardSurface.cols, boardSurface.cellSize);
            container.BindSingleton(grid);

            var projector = new GridProjector(grid, boardSurface);
            container.BindSingleton(projector);

            container.BindSingleton(boardSurface);

            if (hoverHighlightPrefab || placeableCellPrefab)
            {
                var hoverSystem = new PointerHoverSystem(rayProvider, boardSurface, projector, events);
                systems.Register((IUpdatableSystem)hoverSystem);
            }
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
                    hoverGO = Instantiate(hoverHighlightPrefab, visualsRoot, false);
                    hoverGO.name = "HoverHighlight";
                    hoverGO.SetActive(false);
                }

                var visualsSvc = new GridVisualsService(
                    grid, boardSurface, projector, events, placeablePool, hoverGO, visualsRoot, surfaceLift);
                container.BindSingleton(visualsSvc);
            }

            var levelData = levelCatalogue.GetById(levelId);
            if (levelData == null)
            {
                Debug.LogError($"[RuntimeInstaller] Level '{levelId}' not found in LevelCatalogue.");
                return;
            }
            var level = new LevelRuntimeConfig(levelData);
            container.BindSingleton(level);

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

            var pools      = new CharacterPoolRegistry();
            var repo       = new CharacterRepository();
            var factory    = new CharacterFactory(pools);
            var charSystem = new CharacterSystem();

            container.BindSingleton(pools);
            container.BindSingleton(repo);
            container.BindSingleton(factory);
            container.BindSingleton(charSystem);

            systems.Register((IUpdatableSystem)charSystem);

            var validator  = new PlacementValidator(repo, grid);
            container.BindSingleton(validator);

            var previewSvc = new PlacementPreviewService(factory, boardSurface, projector, validator, unitsParent);
            container.BindSingleton(previewSvc);
            

            var spawner     = new CharacterSelectionSpawner(level, characterSelectionSpawnPoint, characterSpacing, selectionParent);
            var selectables = spawner.Spawn(); // List<SelectableCharacterView>

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
            
            var targeting = new Runtime.Combat.TargetingService(grid, repo);
            container.BindSingleton(targeting);

            var lifetime = new Runtime.Combat.CharacterLifetimeSystem(repo, events);
            systems.Register((IUpdatableSystem)lifetime);

            var enemySpawner = new Runtime.Combat.EnemySpawnerSystem(grid, factory, repo, level, unitsParent);
            systems.Register((IUpdatableSystem)enemySpawner);
        }
    }
}
