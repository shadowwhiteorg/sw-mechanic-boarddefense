namespace _Game.Interfaces
{
    public interface IHealth : ICharacterPlugin
    {
        int Current { get; }
        int Max { get; }
        void ApplyDamage(int dmg);
    }
}