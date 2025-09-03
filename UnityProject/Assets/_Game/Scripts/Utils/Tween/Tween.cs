using System;
using System.Collections;
using _Game.Enums;
using UnityEngine;

namespace _Game.Utils
{
    public static class Tween
    {
        public static Coroutine Position(Transform target, Vector3 to, float duration, Ease ease = Ease.Linear, Action onComplete = null)
        {
            return CoroutineRunner.Instance.StartCoroutine(TweenRoutine(
                t => target.position = t, 
                target.position, to, duration, Easing.Get(ease), onComplete));
        }

        public static Coroutine Scale(Transform target, Vector3 to, float duration, Ease ease = Ease.Linear, Action onComplete = null)
        {
            return CoroutineRunner.Instance.StartCoroutine(TweenRoutine(
                t => target.localScale = t,
                target.localScale, to, duration, Easing.Get(ease), onComplete));
        }

        public static Coroutine Rotate(Transform target, Quaternion to, float duration, Ease ease = Ease.Linear, Action onComplete = null)
        {
            return CoroutineRunner.Instance.StartCoroutine(TweenRoutineQuaternion(
                q => target.rotation = q,
                target.rotation, to, duration, Easing.Get(ease), onComplete));
        }
        
        public static Coroutine Float(Action<float> setter, float from, float to, float duration, Ease ease = Ease.Linear, Action onComplete = null)
        {
            return CoroutineRunner.Instance.StartCoroutine(FloatRoutine(setter, from, to, duration, Easing.Get(ease), onComplete));
        }

        private static IEnumerator FloatRoutine(Action<float> setter, float from, float to, float duration, Func<float, float> ease, Action onComplete)
        {
            float time = 0f;
            while (time < duration)
            {
                float t = ease(time / duration);
                setter(Mathf.LerpUnclamped(from, to, t));
                time += Time.deltaTime;
                yield return null;
            }
            setter(to);
            onComplete?.Invoke();
        }


        private static IEnumerator TweenRoutine(Action<Vector3> setter, Vector3 from, Vector3 to, float duration, Func<float, float> ease, Action onComplete)
        {
            float time = 0f;
            while (time < duration)
            {
                float t = ease(time / duration);
                setter(Vector3.LerpUnclamped(from, to, t));
                time += Time.deltaTime;
                yield return null;
            }
            setter(to);
            onComplete?.Invoke();
        }

        private static IEnumerator TweenRoutineQuaternion(Action<Quaternion> setter, Quaternion from, Quaternion to, float duration, Func<float, float> ease, Action onComplete)
        {
            float time = 0f;
            while (time < duration)
            {
                float t = ease(time / duration);
                setter(Quaternion.LerpUnclamped(from, to, t));
                time += Time.deltaTime;
                yield return null;
            }
            setter(to);
            onComplete?.Invoke();
        }
    }
}
