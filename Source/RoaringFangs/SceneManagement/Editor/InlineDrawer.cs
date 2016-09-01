using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.SceneManagement.Editor
{
    public abstract class InlineDrawer : PropertyDrawer
    {
        #region Static Methods

        public static void DrawInlineProperties(Rect position, IEnumerable<SerializedProperty> properties)
        {
            foreach (var inline_property in properties)
            {
                EditorGUI.BeginChangeCheck();
                var inline_label = new GUIContent(inline_property.displayName);
                position.height = EditorGUI.GetPropertyHeight(
                    inline_property,
                    inline_label,
                    inline_property.hasVisibleChildren);
                EditorGUI.PropertyField(
                    position,
                    inline_property,
                    inline_label,
                    inline_property.hasVisibleChildren);
                position.y += position.height;
                if (EditorGUI.EndChangeCheck())
                    inline_property.serializedObject.ApplyModifiedProperties();
            }
        }

        public static float GetPropertiesInlineHeight(IEnumerable<SerializedProperty> properties)
        {
            float height = 0f;
            foreach (var property in properties)
            {
                height += EditorGUI.GetPropertyHeight(property, null, property.hasVisibleChildren);
            }
            return height;
        }

        #endregion Static Methods

        protected abstract IEnumerable<SerializedProperty> GetInlineSerializedProperties(SerializedProperty property);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            if (property.isExpanded)
            {
                position.y += EditorGUIUtility.singleLineHeight;
                var properties = GetInlineSerializedProperties(property);
                DrawInlineProperties(position, properties);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var properties = GetInlineSerializedProperties(property);
            var height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
                height += GetPropertiesInlineHeight(properties);
            return height;
        }
    }
}