using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

using RoaringFangs.Attributes;

namespace RoaringFangs.Editor
{
    [CustomPropertyDrawer(typeof(AutoPropertyAttribute), true)]
    public class AutoPropertyDrawer : PropertyDrawer
    {
        private static readonly char[] AutoPropertyTrimChars = { '_' };
        private PropertyInfo PropertyInfo;
        private object PreviousFieldValue = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();
            var target = property.serializedObject.targetObject;
            var previous_field_value = fieldInfo.GetValue(target);
            // Lazily initialize previous field value
            PreviousFieldValue = PreviousFieldValue ?? previous_field_value;
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndProperty();
            // Whether the serialized property was directly modified (unaffected by undo!)
            bool modified = property.serializedObject.ApplyModifiedProperties();
            // Whether the field value has changed by any means (including undo operations or anything else!)
            bool different = !previous_field_value.Equals(PreviousFieldValue);
            if (modified || different)
            {
                var current_value = fieldInfo.GetValue(target);
                // Restore the previous value so that the property setter can act on changes to the backing field
                if (modified)
                    fieldInfo.SetValue(target, previous_field_value); // Restore to value right before this modification
                else
                    fieldInfo.SetValue(target, PreviousFieldValue); // Restore to value from right after last mofification

                // Lazily initialize the property info
                if (PropertyInfo == null)
                {
                    var auto = (AutoPropertyAttribute)attribute;
                    if (auto.PropertyInfo != null)
                    {
                        PropertyInfo = auto.PropertyInfo;
                    }
                    else
                    {
                        var field_name = fieldInfo.Name;
                        var field_type = fieldInfo.FieldType;
                        var target_type = target.GetType();
                        var auto_property_name = field_name.TrimStart(AutoPropertyTrimChars);
                        var property_info = target_type.GetProperty(auto_property_name, field_type);
                        PropertyInfo = property_info;
                    }
                }

                PropertyInfo.SetValue(target, current_value, null);
                PreviousFieldValue = current_value;
            }
        }
    }
}
