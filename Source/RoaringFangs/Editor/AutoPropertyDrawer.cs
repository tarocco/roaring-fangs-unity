/*
The MIT License (MIT)

Copyright (c) 2016 Roaring Fangs Entertainment

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using UnityEngine;
using UnityEditor;
using System.Reflection;

using RoaringFangs.Attributes;

namespace RoaringFangs.Editor
{
    [CustomPropertyDrawer(typeof(AutoPropertyAttribute), true)]
    public class AutoPropertyDrawer : PropertyDrawer
    {
        private delegate bool PropertyFieldHandler(Rect position, SerializedProperty property, GUIContent label);

        private static readonly char[] AutoPropertyTrimChars = { '_' };
        private PropertyInfo PropertyInfo;
        private object PreviousFieldValue = null;
        private PropertyFieldHandler DrawPropertyField = null;

        #region Field Proxies
        private static bool DelayedIntField(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.DelayedIntField(position, property, label);
            return false;
        }
        private static bool DelayedFloatField(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.DelayedFloatField(position, property, label);
            return false;
        }
        private static bool DelayedTextField(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.DelayedTextField(position, property, label);
            return false;
        }
        #endregion

        /// <summary>
        /// Gets the cortrect property field drawing method for a given SerializedProperty.
        /// </summary>
        /// <param name="delayed">Whether the field should be delayed. See <seealso cref="AutoPropertyAttribute.Delayed"/>.</param>
        private static PropertyFieldHandler GetPropertyFieldDrawer(Rect position, SerializedProperty property, GUIContent label, bool delayed)
        {
            SerializedPropertyType sp_type;
            // If this field should have delayed input
            if (delayed)
                sp_type = property.propertyType; // Type-dependent selection for delayed properties
            else
                sp_type = SerializedPropertyType.Generic; // Default to non-delayed property field
                                                          // Switch to delayed field if Delayed is true
            switch (sp_type)
            {
                case SerializedPropertyType.Integer:
                    return DelayedIntField;
                case SerializedPropertyType.Float:
                    return DelayedFloatField;
                case SerializedPropertyType.String:
                    return DelayedTextField;
                default:
                    return EditorGUI.PropertyField;
            }
        }

        /// <summary>
        /// Gets the target's property info from field info and AutoPropertyAttribute
        /// </summary>
        private static PropertyInfo GetPropertyInfo(Object target, FieldInfo field_info, AutoPropertyAttribute auto)
        {
            if (auto.PropertyInfo != null)
            {
                return auto.PropertyInfo;
            }
            else
            {
                var field_name = field_info.Name;
                var field_type = field_info.FieldType;
                var target_type = target.GetType();
                var auto_property_name = field_name.TrimStart(AutoPropertyTrimChars);
                var property_info = target_type.GetProperty(auto_property_name, field_type);
                return property_info;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //property.serializedObject.Update();
            // Get the target object
            var target = property.serializedObject.targetObject;
            // Get the AutoPropertyDrawer associated with the field
            var auto = (AutoPropertyAttribute)attribute;
            // Get the value of the field before modifying
            var previous_field_value = fieldInfo.GetValue(target);
            // Lazily initialize and cache the previous field value
            PreviousFieldValue = PreviousFieldValue ?? previous_field_value;
            // Lazily initialize snd cache the property field drawer
            DrawPropertyField = DrawPropertyField ?? GetPropertyFieldDrawer(position, property, label, auto.Delayed);
            // Draw the property field
            EditorGUI.BeginProperty(position, label, property);
            DrawPropertyField(position, property, label);
            EditorGUI.EndProperty();
            // Whether the serialized property was directly modified (unaffected by undo!)
            bool modified = property.serializedObject.ApplyModifiedProperties();
            // Whether the field value has changed by any means (including undo operations or anything else!)
            bool different = !Equals(previous_field_value, PreviousFieldValue);
            if (different)
            {
                // Get the value of the field after modifying
                var current_value = fieldInfo.GetValue(target);
                // Restore the field to its previous value so that the property setter can act on changes to the backing field
                if (modified)
                    fieldInfo.SetValue(target, previous_field_value); // Restore to value right before this modification
                else
                    fieldInfo.SetValue(target, PreviousFieldValue); // Restore to value from right after last mofification
                // Lazily initialize the property info
                PropertyInfo = PropertyInfo ?? GetPropertyInfo(target, fieldInfo, auto);
                // Invoke the setter of the property
                PropertyInfo.SetValue(target, current_value, null);
                // Update the previous field value to be the current value
                PreviousFieldValue = current_value;
            }
        }
    }
}
