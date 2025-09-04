using _Game.Interfaces;

namespace _Game.Core.DI
{
    public abstract class BaseInstaller : UnityEngine.MonoBehaviour
    {
        public abstract void Install(IDIContainer container);
    }
}