using System.Collections.Generic;
using UnityEngine;

namespace _Game.Utils
{
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly Queue<T> _pool;
        private readonly T _prefab;
        private readonly Transform _parent;

        public ObjectPool(T prefab, int initialSize, Transform parent = null)
        {
            _prefab = prefab;
            var initialSize1 = Mathf.Max(0, initialSize);
            _parent = parent;
            _pool = new Queue<T>();

            ExpandPool(initialSize1);
        }

        private void ExpandPool(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                T obj = Object.Instantiate(_prefab, _parent);
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public T Get()
        {
            if (_pool.Count == 0) ExpandPool(1);
            T obj = _pool.Dequeue();
            obj.transform.SetParent(_parent, false);
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null) return;
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_parent, false);
            _pool.Enqueue(obj);
        }
    }
}