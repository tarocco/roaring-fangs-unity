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
            bool different = !Equals(previous_field_value, PreviousFieldValue);
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
