using _Game.Interfaces;

namespace _Game.Interfaces
{
    public interface ISystemRunner
    {
        void Register(IUpdatableSystem system);
        void Register(IFixedUpdatableSystem system);
        void Tick();
        void FixedTick();
    }
}