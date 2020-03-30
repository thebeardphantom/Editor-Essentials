using EditorEssentials.Editor;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
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
                            session.ReportErr($"{missingCount} scripts on {gameObject.transform.GetPath()}.", obj);
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