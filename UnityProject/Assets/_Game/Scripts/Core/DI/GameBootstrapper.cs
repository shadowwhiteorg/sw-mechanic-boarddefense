using UnityEngine;
using _Game.Core.DI;
using _Game.Interfaces;

namespace _Game.Core.DI
{
    [DisallowMultipleComponent]
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Installers (in execution order)")]
        [SerializeField] private GameInstaller gameInstaller;       // core
        [SerializeField] private BaseInstaller[] otherInstallers;   // RuntimeInstaller, UIInstaller, ...

        private static GameBootstrapper _instance;
        private DIContainer _container;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _container = new DIContainer();
            _container.BindSingleton<IDIContainer>(_container);
            GameContext.Container = _container;

            if (gameInstaller != null) gameInstaller.Install(_container);

            if (otherInstallers != null)
            {
                foreach (var inst in otherInstallers)
                {
                    if (inst != null) inst.Install(_container);
                }
            }

            if (FindObjectOfType<SystemRunnerBehaviour>() == null)
            {
                var go = new GameObject("SystemRunner");
                DontDestroyOnLoad(go);
                go.AddComponent<SystemRunnerBehaviour>();
            }
        }
    }
}