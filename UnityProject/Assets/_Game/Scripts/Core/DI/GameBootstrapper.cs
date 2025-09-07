using UnityEngine;

namespace _Game.Core.DI
{
    [DisallowMultipleComponent]
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Installers (in execution order)")]
        [SerializeField] private GameInstaller gameInstaller;
        [SerializeField] private RuntimeInstaller runtimeInstaller;
        [SerializeField] private UIInstaller uiInstaller;

        private static GameBootstrapper _instance;

        private void Awake()
        {
            var container = new DIContainer();
            container.BindSingleton(container);
            GameContext.Container = container;

            gameInstaller.Install(container);
            runtimeInstaller.Install(container);
            uiInstaller.Initialize(container, GameContext.Events);
            
            if (FindObjectOfType<SystemRunnerBehaviour>() == null)
            {
                var go = new GameObject("SystemRunner");
                DontDestroyOnLoad(go);
                go.AddComponent<SystemRunnerBehaviour>();
            }
        }
    }
}