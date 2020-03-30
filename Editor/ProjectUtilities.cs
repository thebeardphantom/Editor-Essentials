using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EditorEssentials.Editor
{
    public static class ProjectUtilities
    {
        [MenuItem("Edit/Project Utilities/Scripts/Remove All Missing")]
        public static void RemoveAllMissingScriptsFromProject()
        {
            var output = new StringBuilder();
            output.AppendLine("RemoveAllMissingScriptsFromProject Report");

            var paths = AssetDatabase.FindAssets("t:GameObject")
                .Select(AssetDatabase.GUIDToAssetPath);
            foreach (var path in paths)
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var hierarchy = obj.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject).Distinct();
                foreach (var gameObject in hierarchy)
                {
                    var missingCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                    if (missingCount > 0)
                    {
                        var assetPath = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
                        var fullPath = $"{assetPath}/{gameObject.transform.GetPath()}";
                        output.AppendLine($"\t{missingCount} scripts on {fullPath}.");
                    }
                }
            }
            Debug.LogWarning(output.ToString().Trim());
            AssetDatabase.SaveAssets();
        }
    }
}