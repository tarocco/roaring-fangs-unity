using RoaringFangs.Attributes;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

namespace RoaringFangs.Editor
{
    /*
    [CustomPropertyDrawer(typeof(InlinePropertyInspectorAttribute))]
    public class InlinePropertyInspectorDrawer : PropertyDrawer
    {
        private static SerializedPropertyComparer _SerializedPropertyComparer = new SerializedPropertyComparer();

        private static IEnumerable<SerializedProperty> GetVisibleSubProperties(SerializedProperty property)
        {
            var end_property = property.GetEndProperty(false);
            for(;;)
            {
                yield return property.Copy();
                bool has_next, is_end;
                if (property.type.EndsWith("Event"))
                {
                    has_next = property.NextVisible(false);
                }
                else
                {
                    has_next = property.NextVisible(true);
                }
                is_end = _SerializedPropertyComparer.Compare(property, end_property) == 0;
                if (!has_next || is_end)
                    break;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var serialized_object = property.serializedObject;
            var object_property = serialized_object.GetIterator();
            object_property.NextVisible(true); // Necessary for GetIterator()
            var properties = GetVisibleSubProperties(object_property).ToList();
            Rect position_current = position;
            foreach (var p in properties)
            {
                position_current.height = EditorGUI.GetPropertyHeight(p);
                EditorGUI.PropertyField(position_current, p);
                position_current.y += position_current.height;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
    */
}