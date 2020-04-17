using EditorEssentials.Runtime;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EditorEssentials.Editor
{
    [CustomPropertyDrawer(typeof(EnumMaskAttribute))]
    public class EnumMaskAttributeDrawer : PropertyDrawer
    {
        #region Fields

        private static MethodInfo _getFieldInfoFromPropertyMethod;

        #endregion

        #region Methods

        private static FieldInfo GetFieldInfoFromProperty(SerializedProperty property)
        {
            if (_getFieldInfoFromPropertyMethod == null)
            {
                var editorType = typeof(UnityEditor.Editor);
                var editorAssembly = editorType.Assembly;
                var scriptAttributeUtilityType = editorAssembly.GetType("UnityEditor.ScriptAttributeUtility", true);
                _getFieldInfoFromPropertyMethod = scriptAttributeUtilityType.GetMethod(
                    "GetFieldInfoFromProperty",
                    BindingFlags.Static | BindingFlags.NonPublic);
            }

            var parameters = new object[]
            {
                property,
                null
            };
            return (FieldInfo) _getFieldInfoFromPropertyMethod.Invoke(null, parameters);
        }

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            var field = GetFieldInfoFromProperty(property);
            var targetEnum = (Enum) field.GetValue(property.serializedObject.targetObject);

            EditorGUI.BeginProperty(position, label, property);
            var enumNew = EditorGUI.EnumFlagsField(position, label, targetEnum);

            property.intValue = (int) Convert.ChangeType(
                enumNew,
                targetEnum.GetType());

            EditorGUI.EndProperty();
        }

        #endregion
    }
}