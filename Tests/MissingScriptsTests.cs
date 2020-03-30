using EditorEssentials.Editor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EditorEssentials.Tests.Editor
{
    public class MissingScriptsTests
    {
        #region Methods

        [UnityTest]
        public IEnumerator NoMissingScripts_Prefabs()
        {
            using (var session = new TestSession())
            {
                var paths = AssetDatabase.FindAssets("t:GameObject")
                    .Select(AssetDatabase.GUIDToAssetPath);
                foreach (var path in paths)
                {
                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    var hierarchy = obj.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject).Distinct();
                    foreach (var gameObject in hierarchy)
                    {
                        var missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                        if (missingCount > 0)
                        {
                            var assetPath = Path.GetDirectoryName(path).Replace('\\', '/');
                            var fullPath = $"{assetPath}/{gameObject.transform.GetPath()}";
                            session.ReportErr($"Prefab: {fullPath}\nMissing: {missingCount}", obj, false);
                        }
                    }

                    yield return null;
                }

                yield return Resources.UnloadUnusedAssets();
            }
        }

        [UnityTest]
        public IEnumerator NoMissingScripts_Scenes()
        {
            using (var session = new TestSession())
            {
                var paths = AssetDatabase.FindAssets("t:Scene")
                    .Select(AssetDatabase.GUIDToAssetPath);
                var rootGameObjects = new List<GameObject>();
                foreach (var path in paths)
                {
                    Debug.Log($"Loading scene: {path}");
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
                            var missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                            if (missingCount > 0)
                            {
                                session.ReportErr(
                                    $"Scene: {scene.path}\nObject: {gameObject.transform.GetPath()}\nCount: {missingCount}");
                            }
                        }
                    }

                    EditorSceneManager.CloseScene(scene, true);
                    yield return null;
                }

                yield return Resources.UnloadUnusedAssets();
            }
        }

        #endregion
    }
}