using _Game.Interfaces;
using UnityEngine;

namespace _Game.Runtime.Core
{

    public sealed class ScreenSpaceRayProvider : IRayProvider
    {
        private readonly Camera _cam;
        public ScreenSpaceRayProvider(Camera cam) { _cam = cam; }
        public Ray PointerToRay(Vector2 screenPos) => _cam.ScreenPointToRay(screenPos);
    }
}