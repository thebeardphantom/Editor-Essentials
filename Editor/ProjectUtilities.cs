using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EditorEssentials.Editor
{
    public static class ProjectUtilities
    {
        #region Methods

        [MenuItem("Assets/Editor Essentials/Project Utilities/Remove Missing Scripts")]
        public static void RemoveAllMissingScriptsFromProject()
        {
            if (!EditorUtility.DisplayDialog(
                "Remove Missing Scripts",
                "This will remove all missing scripts from ALL prefabs and scenes, proceed?",
                "Yes",
                "No"))
            {
                return;
            }
            var output = new StringBuilder();
            output.AppendLine("RemoveAllMissingScriptsFromProject Report");
            output.AppendLine("\tPrefabs:");

            var paths = AssetDatabase.FindAssets("t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath);
            foreach (var path in paths)
            {
                var prefabContentsRoot = PrefabUtility.LoadPrefabContents(path);
                var hierarchy = prefabContentsRoot.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject).Distinct();
                foreach (var gameObject in hierarchy)
                {
                    var missingCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                    if (missingCount > 0)
                    {
                        var assetPath = Path.GetDirectoryName(path).Replace('\\', '/');
                        var fullPath = $"{assetPath}/{gameObject.transform.GetPath()}";
                        output.AppendLine($"\t\tPrefab:{fullPath}\n\t\tCount Removed:{missingCount}");
                        EditorUtility.SetDirty(prefabContentsRoot);
                    }
                }

                if (EditorUtility.IsDirty(prefabContentsRoot))
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabContentsRoot, path, out var success);
                    if (!success)
                    {
                        output.AppendLine($"\t\tFAILED TO SAVE PREFAB: {path}");
                    }
                    PrefabUtility.UnloadPrefabContents(prefabContentsRoot);
                }
            }

            // Remove from all scenes
            output.AppendLine("\tScenes");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                paths = AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath);
                var rootGameObjects = new List<GameObject>();
                foreach (var path in paths)
                {
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    var scene = SceneManager.GetSceneByPath(path);
                    rootGameObjects.Clear();
                    scene.GetRootGameObjects(rootGameObjects);
                    foreach (var root in rootGameObjects)
                    {
                        var hierarchy = root.GetComponentsInChildren<Transform>(true)
                            .Select(t => t.gameObject)
                            .Distinct();
                        foreach (var gameObject in hierarchy)
                        {
                            var missingCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                            if (missingCount > 0)
                            {
                                output.AppendLine(
                                    $"\t\tScene: {scene.path}\n\t\tObject: {gameObject.transform.GetPath()}\n\t\tCount Removed: {missingCount}");
                                EditorSceneManager.MarkSceneDirty(scene);
                            }
                        }
                    }

                    EditorSceneManager.SaveScene(scene);
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(setup);
                
                Debug.LogWarning(output.ToString().Trim().Replace("\t", "    "));
                AssetDatabase.SaveAssets();
            }

        }

        #endregion
    }
}