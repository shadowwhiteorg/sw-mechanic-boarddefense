using UnityEngine;

namespace _Game.Utils.Editor.Diagnostics.MissingScript
{
    public class ScanResult
    {
        public string GameObjectPath;
        public string AssetPath; // null if from scene
        public GameObject GameObject;
        public int MissingIndex;

        public string Summary => AssetPath != null
            ? $"Prefab: {AssetPath} ➜ {GameObjectPath}"
            : $"Scene: {GameObjectPath}";
    }
}