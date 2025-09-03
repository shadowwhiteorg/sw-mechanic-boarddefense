using System.Collections.Generic;
using UnityEngine;

namespace _Game.Utils
{
    public class GameObjectPool
    {
        private readonly GameObject   _prefab;
        private readonly Transform    _parent;
        private readonly Stack<GameObject> _available = new Stack<GameObject>();

        public GameObjectPool(GameObject prefab, int initialSize, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < initialSize; i++)
                _available.Push(NewInstance());
        }

        private GameObject NewInstance()
        {
            var go = Object.Instantiate(_prefab, _parent, worldPositionStays:false);
            go.SetActive(false);
            return go;
        }

        public GameObject Get()
        {
            var go = _available.Count > 0 ? _available.Pop() : NewInstance();
            go.SetActive(true);
            return go;
        }

        public void Return(GameObject go)
        {
            go.SetActive(false);
            _available.Push(go);
        }
    }

}