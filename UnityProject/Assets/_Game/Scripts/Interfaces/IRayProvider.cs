using UnityEngine;

namespace _Game.Interfaces
{
    public interface IRayProvider
    {
        Ray PointerToRay(Vector2 screenPos);
    }
}