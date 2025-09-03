using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Game.Utils.Editor.Diagnostics.MissingScript
{
    public static class MissingScriptScanner
    {
        public static List<ScanResult> ScanScene()
        {
            var results = new List<ScanResult>();
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
                ScanRecursive(go, results);
            return results;
        }

        public static List<ScanResult> ScanSceneAsset(SceneAsset sceneAsset)
        {
            string path = AssetDatabase.GetAssetPath(sceneAsset);
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var results = ScanScene();
                EditorSceneManager.CloseScene(scene, true);
                return results;
            }
            return new List<ScanResult>();
        }

        public static List<ScanResult> ScanPrefabs(string folder = "Assets")
        {
            var results = new List<ScanResult>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                    ScanRecursive(prefab, results, path);
            }
            return results;
        }

        private static void ScanRecursive(GameObject go, List<ScanResult> results, string assetPath = null)
        {
            var components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    results.Add(new ScanResult
                    {
                        GameObject = go,
                        GameObjectPath = GetGameObjectPath(go),
                        AssetPath = assetPath,
                        MissingIndex = i
                    });
                }
            }

            foreach (Transform child in go.transform)
                ScanRecursive(child.gameObject, results, assetPath);
        }

        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = obj.name + "/" + path;
            }
            return path;
        }

        public static void CleanMissingComponents(GameObject go)
        {
            if (go == null) return;

            var serializedObject = new SerializedObject(go);
            var property = serializedObject.FindProperty("m_Component");

            for (int i = property.arraySize - 1; i >= 0; i--)
            {
                var element = property.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("component").objectReferenceValue == null)
                {
                    property.DeleteArrayElementAtIndex(i);
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(go);
        }
    }
}
