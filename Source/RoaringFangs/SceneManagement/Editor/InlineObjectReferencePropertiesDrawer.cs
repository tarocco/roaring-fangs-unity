using RoaringFangs.Attributes;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.SceneManagement.Editor
{
    [CustomPropertyDrawer(typeof(InlineScriptableSceneHandlerGroupAttribute))]
    public abstract class InlineObjectReferencePropertiesDrawer : PropertyDrawer
    {
        protected abstract IEnumerable<SerializedProperty> GetInlineSerializedProperties(SerializedObject serialized_object);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var target = property.objectReferenceValue;
            if (target != null)
            {
                var serialized_target = new SerializedObject(target);
                var properties = GetInlineSerializedProperties(serialized_target);
                // Prepend the reference field
                properties = new[] { property }.Concat(properties);
                InlineDrawer.DrawInlineProperties(position, properties);
            }
            else
            {
                position.height = EditorGUI.GetPropertyHeight(property, label);
                EditorGUI.PropertyField(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var target = property.objectReferenceValue;
            float height;
            if (target != null)
            {
                var serialized_target = new SerializedObject(target);
                var properties = GetInlineSerializedProperties(serialized_target);
                // Prepend the reference field
                properties = new[] { property }.Concat(properties);
                height = InlineDrawer.GetPropertiesInlineHeight(properties);
            }
            else
            {
                height = EditorGUI.GetPropertyHeight(property, label);
            }
            height += EditorGUIUtility.singleLineHeight;
            return height;
        }
    }
}