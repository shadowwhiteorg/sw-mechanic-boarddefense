// Assets/_Game/Scripts/Runtime/Combat/GameStateSystem.cs
using UnityEngine;
using _Game.Core.Events;
using _Game.Enums;
using _Game.Interfaces;
using _Game.Runtime.Characters;
using _Game.Runtime.Levels;

namespace _Game.Runtime.Combat
{
    /// Listens to deaths & base damage and announces WIN/LOSE with logs.
    /// WIN condition: all planned enemies (sum of level waves) have been defeated
    ///                AND base HP > 0 at that moment.
    public sealed class GameStateSystem : IUpdatableSystem
    {
        private readonly IEventBus _bus;
        private readonly CharacterRepository _repo;
        private readonly BaseHealthSystem _baseHp;
        private readonly int _plannedEnemies;
        private int _handledEnemies;
        private bool _ended;

        public GameStateSystem(IEventBus bus, CharacterRepository repo, LevelRuntimeConfig level, BaseHealthSystem baseHp)
        {
            _bus = bus; _repo = repo; _baseHp = baseHp;
            _plannedEnemies = CountPlanned(level);

            _bus.Subscribe<CharacterDiedEvent>(OnCharacterDied);
            _bus.Subscribe<GameLostEvent>(_ => _ended = true);
        }

        public void Tick() { /* no-op */ }

        private void OnCharacterDied(CharacterDiedEvent e)
        {
            if (_ended || e.Entity == null) return;
            if (e.Entity.Role != CharacterRole.Enemy) return;

            _handledEnemies++;

            // If all planned enemies are handled and base is alive, we win.
            if (_handledEnemies >= _plannedEnemies && _baseHp.CurrentHp > 0)
            {
                _ended = true;
                Debug.Log($"[GameState] WIN — All {_plannedEnemies} enemies defeated.");
                _bus.Fire(new GameWonEvent());
            }
            else
            {
                Debug.Log($"[GameState] Enemy defeated ({_handledEnemies}/{_plannedEnemies}).");
            }
        }

        private static int CountPlanned(LevelRuntimeConfig level)
        {
            // Sums all enemy counts defined in the active level config.
            // Assumes LevelRuntimeConfig exposes .Waves with .count (int).
            // If your property names differ, adjust here.
            if (level == null || level.Waves == null) return 0;
            int sum = 0;
            for (int i = 0; i < level.Waves.Count; i++)
                sum += Mathf.Max(0, level.Waves[i].count);
            return sum;
        }
    }
}
