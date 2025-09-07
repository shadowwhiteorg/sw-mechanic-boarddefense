using UnityEngine;
using System;
using _Game.Enums;

namespace _Game.Utils
{
    public static class TweenExtensions
    {
        public static Coroutine TweenPosition(this Transform transform, Vector3 to, float duration, Ease ease = Ease.Linear, Action onComplete = null)
            => Tween.Position(transform, to, duration, ease, onComplete);

        public static Coroutine TweenScale(this Transform transform, Vector3 to, float duration, Ease ease = Ease.Linear, Action onComplete = null)
            => Tween.Scale(transform, to, duration, ease, onComplete);
        
        public static Coroutine TweenFloat (this Action<float> setter, float from, float to, float duration, Ease ease = Ease.Linear, Action onComplete = null)
            => Tween.Float(setter, from, to, duration, ease, onComplete);
        
        public static Coroutine TweenRotate(this Transform transform, Quaternion to, float duration, Ease ease = Ease.Linear, Action onComplete = null)
            => Tween.Rotate(transform, to, duration, ease, onComplete);
    }
}