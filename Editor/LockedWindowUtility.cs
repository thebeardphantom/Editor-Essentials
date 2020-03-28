using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EditorEssentials.Editor
{
    public static class LockedWindowUtility
    {
        #region Methods

        [MenuItem("GameObject/Lock Inspector", false, -100)]
        private static void LockGameObjectInspector()
        {
            var selection = Selection.activeGameObject;
            if (selection == null)
            {
                return;
            }

            var instance = GetNewWindowInstance("UnityEditor.InspectorWindow");
            SetProperty(instance, "isLocked", true);
        }

        [MenuItem("CONTEXT/Component/Lock Inspector", false, -100)]
        private static void LockComponentInspector(MenuCommand cmd)
        {
            var instance = GetNewWindowInstance("UnityEditor.InspectorWindow");
            SetProperty(instance, "isLocked", true);
        }

        [MenuItem("Assets/Lock Inspector", false, -100)]
        private static void LockAssetInspector()
        {
            var selection = Selection.activeObject;
            if (selection == null || !EditorUtility.IsPersistent(selection))
            {
                return;
            }

            var instance = GetNewWindowInstance("UnityEditor.InspectorWindow");
            SetProperty(instance, "isLocked", true);
        }

        [MenuItem("Assets/Lock Project Browser", false, -100)]
        private static void LockAssetProjectBrowser()
        {
            var selection = Selection.activeObject;
            if (selection == null || !EditorUtility.IsPersistent(selection))
            {
                return;
            }

            var instance = GetNewWindowInstance("UnityEditor.ProjectBrowser");
            EditorApplication.delayCall += () =>
            {
                SetProperty(instance, "isLocked", true);
            };
        }

        private static void SetProperty(object obj, string propName, object value)
        {
            var prop = obj.GetType()
                .GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            prop.SetValue(obj, value);
        }

        private static EditorWindow GetNewWindowInstance(string fqtn)
        {
            var type = typeof(EditorWindow).Assembly.GetType(fqtn);
            if (type == null)
            {
                throw new Exception($"Type not found: {fqtn}");
            }

            var instance = EditorWindow.GetWindow(type);
            if (instance == null)
            {
                instance = (EditorWindow) ScriptableObject.CreateInstance(type);
            }
            else
            {
                instance = Object.Instantiate(instance);
            }

            instance.Show(true);
            return instance;
        }

        #endregion
    }
}