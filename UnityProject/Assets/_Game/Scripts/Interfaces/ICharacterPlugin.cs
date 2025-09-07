using _Game.Runtime.Characters;

namespace _Game.Interfaces
{
    public interface ICharacterPlugin
    {
        void OnSpawn(CharacterEntity e);
        void Tick(float dt);
        void OnDespawn();
    }
}