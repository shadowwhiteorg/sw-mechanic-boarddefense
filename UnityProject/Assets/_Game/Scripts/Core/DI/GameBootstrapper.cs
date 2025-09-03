using UnityEngine;

namespace _Game.Core.DI
{
    public class GameBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            var container = new DIContainer();
        }
    }
}