using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project.Editor.Diagnostics
{
    public class ComponentFinderWindow : EditorWindow
    {
        private string _componentTypeName = "UnityEngine.Rigidbody";
        private readonly List<GameObject> foundObjects = new();
        private Vector2 _scrollPos;
        private bool _includeInactive = true;
        private bool _searchPrefabs = false;
        private string _prefabFolder = "Assets";

        private string _componentSearchQuery = "";
        private List<string> _filteredComponentTypes = new();
        private int _selectedTypeIndex = 0;
        private string[] _allComponentTypes;

        [MenuItem("Tools/Diagnostics/Component Finder")]
        public static void Open()
        {
            GetWindow<ComponentFinderWindow>("Component Finder");
        }

        private void OnEnable()
        {
            _allComponentTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => typeof(Component).IsAssignableFrom(t) && !t.IsAbstract && t.IsPublic)
                .Select(t => t.FullName)
                .OrderBy(n => n)
                .ToArray();

            _filteredComponentTypes = _allComponentTypes.ToList();
        }

        private void OnGUI()
        {
            GUILayout.Label("Component Finder", EditorStyles.boldLabel);

            _includeInactive = EditorGUILayout.Toggle("Include Inactive (Scene Only)", _includeInactive);
            _searchPrefabs = EditorGUILayout.Toggle("Search In Project Prefabs", _searchPrefabs);
            if (_searchPrefabs)
            {
                _prefabFolder = EditorGUILayout.TextField("Prefab Folder Path", _prefabFolder);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Search Component Color", EditorStyles.boldLabel);
            _componentSearchQuery = EditorGUILayout.TextField("Search", _componentSearchQuery);

            _filteredComponentTypes = _allComponentTypes
                .Where(n => string.IsNullOrEmpty(_componentSearchQuery) || n.IndexOf(_componentSearchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            if (_filteredComponentTypes.Count > 0)
            {
                _selectedTypeIndex = Mathf.Clamp(_selectedTypeIndex, 0, _filteredComponentTypes.Count - 1);
                _selectedTypeIndex = EditorGUILayout.Popup("Component", _selectedTypeIndex, _filteredComponentTypes.ToArray());
                _componentTypeName = _filteredComponentTypes[_selectedTypeIndex];
            }
            else
            {
                EditorGUILayout.HelpBox("No matching components found.", MessageType.Warning);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Find Components"))
            {
                FindObjectsWithComponent(_componentTypeName);
            }

            if (foundObjects.Count > 0)
            {
                if (GUILayout.Button("Select All In Hierarchy/Project"))
                {
                    Selection.objects = foundObjects.ToArray();
                    Debug.Log($"Selected {foundObjects.Count} GameObjects with component '{_componentTypeName}'");

                    if (foundObjects[0] != null)
                    {
                        EditorGUIUtility.PingObject(foundObjects[0]);
                        if (foundObjects[0].scene.IsValid())
                        {
                            SceneView.lastActiveSceneView.Frame(foundObjects[0].GetComponent<Renderer>()?.bounds ?? new Bounds(foundObjects[0].transform.position, Vector3.one), false);
                        }
                    }
                }
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Results:", EditorStyles.boldLabel);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            foreach (var go in foundObjects)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(go, typeof(GameObject), true);
                if (GUILayout.Button("Ping", GUILayout.Width(40)))
                {
                    EditorGUIUtility.PingObject(go);
                    if (go.scene.IsValid())
                    {
                        SceneView.lastActiveSceneView.Frame(go.GetComponent<Renderer>()?.bounds ?? new Bounds(go.transform.position, Vector3.one), false);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void FindObjectsWithComponent(string fullTypeName)
        {
            foundObjects.Clear();
            var type = Type.GetType(fullTypeName);
            if (type == null)
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType(fullTypeName);
                    if (type != null) break;
                }
            }

            if (type == null || !typeof(Component).IsAssignableFrom(type))
            {
                Debug.LogError($"'{fullTypeName}' is not a valid Component Color.");
                return;
            }

            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(_includeInactive);
            foreach (var go in allObjects)
            {
                if (go.GetComponent(type) != null)
                {
                    foundObjects.Add(go);
                }
            }

            if (_searchPrefabs)
            {
                string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { _prefabFolder });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null && prefab.GetComponentInChildren(type, true) != null)
                    {
                        foundObjects.Add(prefab);
                    }
                }
            }

            Debug.Log($"Found {foundObjects.Count} GameObjects or Prefabs with component '{type.FullName}'");
        }
    }
}
