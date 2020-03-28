using EditorEssentials.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tests;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace EditorEssentials.Tests.Editor
{
    public class ObjectReferenceTests
    {
        #region Types

        private class FieldData
        {
            #region Fields

            public readonly FieldInfo Info;

            public readonly string Path;

            #endregion

            #region Constructors

            public FieldData(FieldInfo info, string path)
            {
                Info = info;
                Path = $"{path}{info.Name}";
            }

            #endregion
        }

        #endregion

        #region Fields

        private const BindingFlags FIELD_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly List<FieldInfo> _fields
            = new List<FieldInfo>();

        private readonly List<MonoBehaviour> _monoBehaviours
            = new List<MonoBehaviour>();

        private readonly List<FieldData> _fieldsWithMissingValues
            = new List<FieldData>();

        #endregion

        #region Methods

        private static bool ShouldIgnoreComponent(Component c)
        {
            if (c == null)
            {
                return true;
            }

            var fullName = c.GetType().FullName;
            return fullName.StartsWith("UnityEngine.")
                || fullName.StartsWith("TMPro.");
        }

        private static bool IsNull(object obj)
        {
            switch (obj)
            {
                case null:
                {
                    return true;
                }
                case Object unityObj:
                {
                    return unityObj == null;
                }
                default:
                {
                    return false;
                }
            }
        }

        private static bool IsFieldWithMissingValue(FieldInfo field, object obj)
        {
            return field.GetCustomAttribute<NonNullableAttribute>(true) != null
                && IsFieldValueNull(field, obj);
        }

        private static bool IsFieldValueNull(FieldInfo field, object obj)
        {
            var value = field.GetValue(obj);
            var isFieldValueNull = IsNull(value);
            return isFieldValueNull;
        }

        private static bool IsListType(FieldInfo field, object obj)
        {
            var isList = typeof(IList).IsAssignableFrom(field.FieldType);
            var isNull = IsFieldValueNull(field, obj);
            return isList && !isNull;
        }

        [UnityTest]
        public IEnumerator NoMissingObjectReferences_ScriptableObjects()
        {
            using (var session = new TestSession())
            {
                var paths = AssetDatabase.FindAssets("t:ScriptableObject")
                    .Select(AssetDatabase.GUIDToAssetPath);
                foreach (var path in paths)
                {
                    var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    _fieldsWithMissingValues.Clear();
                    GetFieldsWithMissingValuesRecursive(obj, "");
                    foreach (var field in _fieldsWithMissingValues)
                    {
                        session.ReportErr(
                            $"Missing ObjRef:\n\tAsset: {path}\n\tField: {field.Path}",
                            obj,
                            false);
                    }

                    yield return null;
                }

                yield return Resources.UnloadUnusedAssets();
            }
        }

        [UnityTest]
        public IEnumerator NoMissingObjectReferences_Prefabs()
        {
            using (var session = new TestSession())
            {
                var paths = AssetDatabase.FindAssets("t:GameObject")
                    .Select(AssetDatabase.GUIDToAssetPath);
                foreach (var path in paths)
                {
                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    ReportMissingObjectRefs(obj, session);
                    yield return null;
                }

                yield return Resources.UnloadUnusedAssets();
            }
        }

        [UnityTest]
        public IEnumerator NoMissingObjectReferences_Scenes()
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
                        ReportMissingObjectRefs(root, session);
                    }

                    EditorSceneManager.CloseScene(scene, true);
                    yield return null;
                }

                yield return Resources.UnloadUnusedAssets();
            }
        }

        private void GetFieldsUpwards(Type type, BindingFlags flags)
        {
            _fields.Clear();
            var currentType = type;
            while (currentType != null)
            {
                _fields.AddRange(currentType.GetFields(flags));
                currentType = currentType.BaseType;
            }
        }

        private void GetFieldsWithMissingValuesRecursive(object obj, string fieldPath)
        {
            if (obj == null)
            {
                return;
            }

            GetFieldsUpwards(obj.GetType(), FIELD_FLAGS);
            var fieldsWithMissingValues = _fields
                .Where(field => IsFieldWithMissingValue(field, obj))
                .Select(field => new FieldData(field, fieldPath));
            _fieldsWithMissingValues.AddRange(fieldsWithMissingValues);

            var collectionFields = _fields
                .Where(field => IsListType(field, obj))
                .ToArray();
            foreach (var collectionField in collectionFields)
            {
                var list = (IList) collectionField.GetValue(obj);
                for (var i = 0; i < list.Count; i++)
                {
                    var val = list[i];
                    var nextFieldPath = $"{collectionField.Name}[{i}].";
                    GetFieldsWithMissingValuesRecursive(val, nextFieldPath);
                }
            }
        }

        private void ReportMissingObjectRefs(
            GameObject rootObj,
            TestSession session)
        {
            rootObj = rootObj.transform.root.gameObject;
            _monoBehaviours.Clear();
            rootObj.GetComponentsInChildren(true, _monoBehaviours);
            if (_monoBehaviours.Any(m => m == null))
            {
                session.ReportErr($"Null MB on '{rootObj}'", rootObj);
            }

            _monoBehaviours.RemoveAll(ShouldIgnoreComponent);
            var rootObjScene = rootObj.scene;
            foreach (var monoBehaviour in _monoBehaviours)
            {
                _fieldsWithMissingValues.Clear();
                GetFieldsWithMissingValuesRecursive(monoBehaviour, "");
                var cmpPath = monoBehaviour.GetPath();
                foreach (var field in _fieldsWithMissingValues)
                {
                    session.ReportErr(
                        rootObjScene.IsValid()
                            ? $"Missing ObjRef:\n\tScene: {rootObjScene.path}\n\tGameObject: {cmpPath}\n\tComponent: {monoBehaviour.GetType()}\n\tField: {field.Path}"
                            : $"Missing ObjRef:\n\tGameObject: {cmpPath}\n\tComponent: {monoBehaviour.GetType()}\n\tField: {field.Path}",
                        monoBehaviour,
                        false);
                }
            }
        }

        #endregion
    }
}