using _Game.Interfaces;
using _Game.Runtime.Characters;

namespace _Game.Runtime.Characters.Plugins
{
    public sealed class HealthPlugin : IHealth
    {
        private CharacterEntity _entity;
        public int Current { get; private set; }
        public int Max { get; }

        public HealthPlugin(int max)
        {
            Max = max < 1 ? 1 : max;
            Current = Max;
        }

        public void OnSpawn(CharacterEntity e) { _entity = e; }
        public void Tick(float dt) { }

        public void ApplyDamage(int dmg)
        {
            if (dmg <= 0) return;
            Current -= dmg;
            if (Current <= 0)
            {
                Current = 0;
                // TODO: fire CharacterDiedEvent & return entity to pool via a Despawn flow
            }
        }

        public void OnDespawn() { _entity = null; }
    }
}