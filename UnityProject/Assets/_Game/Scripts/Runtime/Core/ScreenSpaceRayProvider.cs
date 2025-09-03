using UnityEngine;

namespace _Game.Runtime.Core
{
    public interface IRayProvider
    {
        Ray PointerToRay(Vector2 screenPos);
    }

    /// <summary>
    /// Simple Camera-based ray provider. Plug in Main Camera or a specific one.
    /// </summary>
    public sealed class ScreenSpaceRayProvider : IRayProvider
    {
        private readonly Camera _cam;
        public ScreenSpaceRayProvider(Camera cam) { _cam = cam; }
        public Ray PointerToRay(Vector2 screenPos) => _cam.ScreenPointToRay(screenPos);
    }
}