using UnityEngine;
using _Game.Interfaces;
using _Game.Runtime.Board;
using _Game.Runtime.Core;
using _Game.Runtime.Systems;

namespace _Game.Core.DI
{
    public sealed class RuntimeInstaller : BaseInstaller
    {
        [Header("Scene References")]
        [SerializeField] private BoardSurface boardSurface;
        [SerializeField] private Camera targetCamera;

        public override void Install(IDIContainer container)
        {
            // 1) Bind IRayProvider
            if (targetCamera == null) targetCamera = Camera.main;
            var rayProvider = new ScreenSpaceRayProvider(targetCamera);
            container.BindSingleton<IRayProvider>(rayProvider);

            // 2) Bind BoardGrid (authoritative board) and GridProjector
            var grid = new BoardGrid(boardSurface.rows, boardSurface.cols, boardSurface.cellSize);
            container.BindSingleton(grid);
            var projector = new GridProjector(grid, boardSurface);
            container.BindSingleton(projector);
            container.BindSingleton(boardSurface);

            // 3) Register runtime systems into the runner
            var systems = _Game.Core.GameContext.Systems;
            var events  = _Game.Core.GameContext.Events;

            var hoverSystem = new PointerHoverSystem(rayProvider, boardSurface, projector, events);
            systems.Register((IUpdatableSystem)hoverSystem);
        }
    }
}