using UnityEngine;

namespace _Game.Utils
{
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("CoroutineRunner");
                    DontDestroyOnLoad(obj);
                    _instance = obj.AddComponent<CoroutineRunner>();
                }
                return _instance;
            }
        }
    }
}