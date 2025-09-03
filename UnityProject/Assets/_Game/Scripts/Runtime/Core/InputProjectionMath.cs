using UnityEngine;

namespace _Game.Runtime.Core
{
    public static class InputProjectionMath
    {
        public static bool TryRayPlane(in Ray ray, in Vector3 p0, in Vector3 n, out Vector3 hitWorld, float minT = 0.0001f)
        {
            hitWorld = default;
            float denom = Vector3.Dot(n, ray.direction);
            if (Mathf.Abs(denom) < 1e-6f) return false; // parallel
            float t = Vector3.Dot(n, (p0 - ray.origin)) / denom;
            if (t < minT) return false;
            hitWorld = ray.origin + t * ray.direction;
            return true;
        }
    }
}