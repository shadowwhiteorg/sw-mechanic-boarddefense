using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace _Game.Utils.Editor.Diagnostics.MissingScript
{
    public class MissingScriptFinderWindow : EditorWindow
    {
        private readonly List<ScanResult> _results = new();
        private Vector2 _scrollPos;
        private bool _searchInPrefabs = true;
        private bool _searchInScenes = true;
        private string _folderFilter = "Assets";
        private readonly List<SceneAsset> _scenesToScan = new();

        [MenuItem("Tools/Diagnostics/Missing Script Finder")]
        public static void Open()
        {
            GetWindow<MissingScriptFinderWindow>("Missing Scripts");
        }

        private void OnGUI()
        {
            GUILayout.Label("Missing Script Scanner", EditorStyles.boldLabel);
            _searchInScenes = GUILayout.Toggle(_searchInScenes, "Include Open Scene");
            _searchInPrefabs = GUILayout.Toggle(_searchInPrefabs, "Include Project Prefabs");

            EditorGUILayout.Space();
            _folderFilter = EditorGUILayout.TextField("Folder Filter (for Prefabs)", _folderFilter);

            EditorGUILayout.LabelField("Batch Scene Scan:");
            int sceneCount = Mathf.Max(0, EditorGUILayout.IntField("Scene Count", _scenesToScan.Count));
            while (_scenesToScan.Count < sceneCount)
                _scenesToScan.Add(null);
            while (_scenesToScan.Count > sceneCount)
                _scenesToScan.RemoveAt(_scenesToScan.Count - 1);

            for (int i = 0; i < _scenesToScan.Count; i++)
            {
                _scenesToScan[i] = (SceneAsset)EditorGUILayout.ObjectField($"Scene {i + 1}", _scenesToScan[i], typeof(SceneAsset), false);
            }

            if (GUILayout.Button("Scan"))
            {
                _results.Clear();
                if (_searchInScenes)
                    _results.AddRange(MissingScriptScanner.ScanScene());
                if (_searchInPrefabs)
                    _results.AddRange(MissingScriptScanner.ScanPrefabs(_folderFilter));
                foreach (var sceneAsset in _scenesToScan)
                {
                    if (sceneAsset != null)
                        _results.AddRange(MissingScriptScanner.ScanSceneAsset(sceneAsset));
                }
            }

            if (GUILayout.Button("Clean All"))
            {
                foreach (var result in _results)
                    MissingScriptScanner.CleanMissingComponents(result.GameObject);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[MissingScriptFinder] Cleaned {_results.Count} Entries.");
                _results.Clear();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Export Results to CSV"))
                ExportResultsToCSV();

            if (GUILayout.Button("Export Results to JSON"))
                ExportResultsToJSON();

            EditorGUILayout.Space();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            foreach (var result in _results)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(result.GameObject, typeof(GameObject), true);
                EditorGUILayout.LabelField(result.Summary, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void ExportResultsToCSV()
        {
            string path = EditorUtility.SaveFilePanel("Export Missing Script Report to CSV", "", "MissingScriptReport.csv", "csv");
            if (string.IsNullOrEmpty(path)) return;

            using var writer = new StreamWriter(path);
            writer.WriteLine("AssetPath,GameObjectPath,MissingIndex");
            foreach (var r in _results)
                writer.WriteLine($"{r.AssetPath},{r.GameObjectPath},{r.MissingIndex}");
            Debug.Log($"Exported to CSV: {path}");
        }

        private void ExportResultsToJSON()
        {
            string path = EditorUtility.SaveFilePanel("Export Missing Script Report to JSON", "", "MissingScriptReport.json", "json");
            if (string.IsNullOrEmpty(path)) return;

            var exportList = new List<SerializableScanResult>();
            foreach (var r in _results)
            {
                exportList.Add(new SerializableScanResult
                {
                    GameObjectPath = r.GameObjectPath,
                    AssetPath = r.AssetPath,
                    MissingIndex = r.MissingIndex
                });
            }

            string json = JsonUtility.ToJson(new ScanResultWrapper { Items = exportList.ToArray() }, true);
            File.WriteAllText(path, json);
            Debug.Log($"Exported to JSON: {path}");
        }

        [Serializable]
        public class SerializableScanResult
        {
            public string GameObjectPath;
            public string AssetPath;
            public int MissingIndex;
        }

        [Serializable]
        public class ScanResultWrapper
        {
            public SerializableScanResult[] Items;
        }

        [MenuItem("Tools/Diagnostics/CLI Scan Export JSON")]
        public static void CLIExportJson()
        {
            var allResults = new List<ScanResult>();
            allResults.AddRange(MissingScriptScanner.ScanScene());
            allResults.AddRange(MissingScriptScanner.ScanPrefabs());

            var exportList = new List<SerializableScanResult>();
            foreach (var r in allResults)
            {
                exportList.Add(new SerializableScanResult
                {
                    GameObjectPath = r.GameObjectPath,
                    AssetPath = r.AssetPath,
                    MissingIndex = r.MissingIndex
                });
            }

            string outputPath = "Assets/Editor/Diagnostics/MissingScriptReport_CLI.json";
            string json = JsonUtility.ToJson(new ScanResultWrapper { Items = exportList.ToArray() }, true);
            File.WriteAllText(outputPath, json);
            Debug.Log($"CLI Export complete to: {outputPath}");
        }
    }
}
