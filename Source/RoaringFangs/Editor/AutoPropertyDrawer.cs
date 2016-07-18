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
using System.Linq;

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
        private static bool RangeField(Rect position, SerializedProperty property, GUIContent label, float min, float max)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    property.floatValue = EditorGUI.Slider(position, label, property.floatValue, min, max);
                    return false;
                case SerializedPropertyType.Integer:
                    property.intValue = EditorGUI.IntSlider(position, label, property.intValue, Mathf.FloorToInt(min), Mathf.FloorToInt(max));
                    return false;
            }
            return EditorGUI.PropertyField(position, property, label, true);
        }
        private static bool MinMaxField(Rect position, SerializedProperty property, GUIContent label, float min, float max)
        {
            Vector2 vector2Value; 
            float value_min, value_max;
            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2:
                    vector2Value = property.vector2Value;
                    value_min = vector2Value.x;
                    value_max = vector2Value.y;
                    EditorGUI.MinMaxSlider(label, position, ref value_min, ref value_max, min, max);
                    property.vector2Value = new Vector2(value_min, value_max);
                    return false;
            }
            return EditorGUI.PropertyField(position, property, label, true);
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
        private static PropertyFieldHandler GetPropertyFieldDrawer(Type property_type, bool delayed, AutoPropertyAttribute auto)
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
            foreach(var _attribute in auto.PropertyAttributes)
            {
                // Curry time!
                if (_attribute is RangeAttribute)
                {
                    var range_attribute = _attribute as RangeAttribute;
                    return (a, b, c) => RangeField(a, b, c, range_attribute.min, range_attribute.max);
                }
                else if (_attribute is MinMaxAttribute)
                {
                    var min_max_attribute = _attribute as MinMaxAttribute;
                    return (a, b, c) => MinMaxField(a, b, c, min_max_attribute.Min, min_max_attribute.Max);
                }
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
            // Lazily initialize/cache the property attributes
            auto.PropertyAttributes = /*auto.PropertyAttributes ??*/ Attribute.GetCustomAttributes(auto.FieldInfo);
            // Lazily initialize/cache the property field drawer for this property
            _DrawPropertyField = /*_DrawPropertyField ??*/ GetPropertyFieldDrawer(auto.PropertyInfo.DeclaringType, auto.Delayed, auto);
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
