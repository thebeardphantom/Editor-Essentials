using EditorEssentials.Editor;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
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
                            var assetPath = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
                            var fullPath = $"{assetPath}/{gameObject.transform.GetPath()}";
                            session.ReportErr($"{missingCount} scripts on {fullPath}.", obj, false);
                        }
                    }

                    yield return null;
                }

                yield return Resources.UnloadUnusedAssets();
            }
        }

        #endregion
    }
}