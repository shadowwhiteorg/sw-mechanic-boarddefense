namespace _Game.Interfaces
{
    public interface IDIContainer
    {
        void Bind<TInterface, TImplementation>() where TImplementation : TInterface;
        void BindSingleton<T>(T instance);
        T Resolve<T>();
    }
}