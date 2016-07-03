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
using System.Collections.Generic;
using System.Reflection;

using RoaringFangs.Attributes;

namespace RoaringFangs.Editor
{
    [InitializeOnLoad]
    [CustomPropertyDrawer(typeof(AutoPropertyAttribute), true)]
    public class AutoPropertyDrawer : PropertyDrawer
    {
        private delegate bool PropertyFieldHandler(Rect position, SerializedProperty property, GUIContent label);

        /// <summary>
        /// Draw method to use in OnGUI for this property
        /// </summary>
        private PropertyFieldHandler _DrawPropertyField = null;

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
        private static bool PropertyFieldIncludingChildren(Rect position, SerializedProperty property, GUIContent label)
        {
            return EditorGUI.PropertyField(position, property, label, true);
        }
        #endregion

        /// <summary>
        /// Gets the cortrect property field drawing method for a given SerializedProperty.
        /// </summary>
        /// <param name="delayed">Whether the field should be delayed. See <seealso cref="AutoPropertyAttribute.Delayed"/>.</param>
        private static PropertyFieldHandler GetPropertyFieldDrawer(SerializedPropertyType sp_type, bool delayed)
        {
            if (delayed)
            {
                switch (sp_type)
                {
                    case SerializedPropertyType.Integer:
                        return DelayedIntField;
                    case SerializedPropertyType.Float:
                        return DelayedFloatField;
                    case SerializedPropertyType.String:
                        return DelayedTextField;
                }
            }
            return PropertyFieldIncludingChildren;
        }

        /// <summary>
        /// Gets the cortrect property field drawing method for a given SerializedProperty.
        /// </summary>
        /// <param name="delayed">Whether the field should be delayed. See <seealso cref="AutoPropertyAttribute.Delayed"/>.</param>
        private static PropertyFieldHandler GetPropertyFieldDrawer(Type property_type, bool delayed)
        {
            if (delayed)
            {
                if (property_type == typeof(int) || property_type == typeof(long))
                    return DelayedIntField;
                else if (property_type == typeof(float) || property_type == typeof(double))
                    return DelayedFloatField;
                else if (property_type == typeof(string))
                    return DelayedTextField;
            }
            return PropertyFieldIncludingChildren;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get the AutoPropertyDrawer associated with the field
            var auto = (AutoPropertyAttribute)attribute;
            // Get the serialized object the serialized property belongs to
            var serialized_object = property.serializedObject;
            // Lazily initialize/cache the field info to the attribute
            auto.FieldInfo = auto.FieldInfo ?? fieldInfo;
            // Lazily initialize/cache the property info to the attribute
            auto.PropertyInfo = auto.PropertyInfo ?? AutoPropertyAttribute.GetPropertyInfoAuto(fieldInfo);
            // Lazily initialize/cache the property field drawer for this property
            _DrawPropertyField = _DrawPropertyField ?? GetPropertyFieldDrawer(auto.PropertyInfo.DeclaringType, auto.Delayed);
            // Begin checking for changes
            EditorGUI.BeginChangeCheck();
            // Draw the property field
            label = EditorGUI.BeginProperty(position, label, property);
            _DrawPropertyField(position, property, label);
            EditorGUI.EndProperty();
            // Whether the field value has changed
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var target in serialized_object.targetObjects)
                {
                    auto.Validate(serialized_object, target, true);
                }
            }
        }

        public AutoPropertyDrawer() :
            base()
        {
        }
    }
}
