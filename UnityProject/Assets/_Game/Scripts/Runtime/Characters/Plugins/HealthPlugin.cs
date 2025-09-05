using _Game.Core;
using _Game.Core.Events;
using _Game.Interfaces;

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
        public void OnDespawn() { _entity = null; }
        public void Tick(float dt) { }

        public void ApplyDamage(int dmg)
        {
            if (dmg <= 0 || _entity == null) return;
            Current -= dmg;
            if (Current > 0) return;

            Current = 0;
            GameContext.Events?.Fire(new CharacterDiedEvent(_entity));
        }
    }
}