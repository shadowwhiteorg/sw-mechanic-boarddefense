using UnityEngine;

namespace _Game.Core
{
    public sealed class SystemRunnerBehaviour : MonoBehaviour
    {
        private void Update() => GameContext.Systems?.Tick();
        private void FixedUpdate() => GameContext.Systems?.FixedTick();
    }
}