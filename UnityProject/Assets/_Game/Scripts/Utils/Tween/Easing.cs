using System;
using _Game.Enums;

namespace _Game.Utils
{
    public static class Easing
    {
        public static float Linear(float t) => t;
        public static float EaseInQuad(float t) => t * t;
        public static float EaseOutQuad(float t) => t * (2 - t);
        public static float EaseInOutQuad(float t) => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;

        public static Func<float, float> Get(Ease ease) => ease switch
        {
            Ease.Linear => Linear,
            Ease.InQuad => EaseInQuad,
            Ease.OutQuad => EaseOutQuad,
            Ease.InOutQuad => EaseInOutQuad,
            _ => Linear
        };
    }

}