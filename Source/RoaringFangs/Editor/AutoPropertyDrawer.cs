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

        private struct PropertyInfoBinding
        {
            public int TargetId;
            public PropertyInfo PropertyInfo;
        }

        /// <summary>
        /// Characters to trim from field names when searching for corresponding properties
        /// </summary>
        private static readonly char[] _AutoPropertyTrimChars = { '_' };
        /// <summary>
        /// PropertyInfo for property associated with the field with this drawer's AutoPropertyAttribute
        /// </summary>
        private PropertyInfo _PropertyInfo;
        /// <summary>
        /// Previously set value of the field with this drawer's AutoPropertyAttribute
        /// </summary>
        private object _PreviouslySetFieldValue = null;
        /// <summary>
        /// Draw method to use in OnGUI for this property
        /// </summary>
        private PropertyFieldHandler _DrawPropertyField = null;
        /// <summary>
        /// Weak reference to the last-used serialized property's serialized object's target
        /// </summary>
        private WeakReference _TargetWR = new WeakReference(null);

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

        /// <summary>
        /// Gets the target's property info from field info and AutoPropertyAttribute
        /// </summary>
        private static PropertyInfo GetPropertyInfo(FieldInfo field_info, AutoPropertyAttribute auto)
        {
            if (auto != null && auto.PropertyInfo != null)
            {
                return auto.PropertyInfo;
            }
            else
            {
                var field_name = field_info.Name;
                var field_type = field_info.FieldType;
                var field_declaring_type = field_info.DeclaringType;
                var auto_property_name = field_name.TrimStart(_AutoPropertyTrimChars);
                var property_info = field_declaring_type.GetProperty(auto_property_name, field_type);
                return property_info;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OnGUI(position, property, label, false);
        }

        private void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool commit)
        {
            // Get the AutoPropertyDrawer associated with the field
            var auto = (AutoPropertyAttribute)attribute;
            // Lazily initialize and cache the property info
            _PropertyInfo = _PropertyInfo ?? GetPropertyInfo(fieldInfo, auto);
            // Lazily initialize and cache the property field drawer
            _DrawPropertyField = _DrawPropertyField ?? GetPropertyFieldDrawer(_PropertyInfo.DeclaringType, auto.Delayed);
            // Get the serialized object the serialized property belongs to
            var serialized_object = property.serializedObject;
            // Get the target object
            var target = serialized_object.targetObject;
            // Get the value of the field before modifying
            var field_value = fieldInfo.GetValue(target);
            // Begin checking for changes
            EditorGUI.BeginChangeCheck();
            // Draw the property field
            label = EditorGUI.BeginProperty(position, label, property);
            _DrawPropertyField(position, property, label);
            EditorGUI.EndProperty();
            // Whether the field value has changed
            if (EditorGUI.EndChangeCheck())
            {
                _PreviouslySetFieldValue = UpdatePropertyFromField(fieldInfo, _PropertyInfo, serialized_object, _PreviouslySetFieldValue, true);
            }
            _TargetWR.Target = target;
            UpdateLateBindings(target);
        }

        private static object UpdatePropertyFromField(
            FieldInfo field_info,
            PropertyInfo property_info,
            SerializedObject serialized_object,
            object previously_set_field_value,
            bool with_undo)
        {
            // Target of the serialized object
            UnityEngine.Object target = serialized_object.targetObject;
            // Get the value of the field before modifying
            var field_value_before_apply = field_info.GetValue(target);
            // Whether the serialized property was directly modified
            bool modified;
            if (with_undo)
                modified = serialized_object.ApplyModifiedProperties();
            else
                modified = serialized_object.ApplyModifiedPropertiesWithoutUndo();
            object previous_value;
            if (modified)
                previous_value = field_value_before_apply; // Restore to value right before this application
            else
                previous_value = previously_set_field_value; // Restore to value from right after previous application
            // Get the value of the field after applying modified properties
            var current_value = field_info.GetValue(target);
            // Restore the field to its previous value so that the property setter can act on changes to the backing field
            field_info.SetValue(target, previous_value);
            // Invoke the setter of the property
            property_info.SetValue(target, current_value, null);
            // Return the current value
            return current_value;
        }

        private void UpdateLateBindings(object target)
        {
            // Perform late binding on target and handlers
            WeakReference previous_drawer_wr;
            var key = new PropertyInfoBinding()
            {
                TargetId = target.GetHashCode(),
                PropertyInfo = _PropertyInfo,
            };
            if (AutoPropertyDrawerBindings.TryGetValue(key, out previous_drawer_wr) && previous_drawer_wr.IsAlive)
            {
                var previous_drawer = previous_drawer_wr.Target as AutoPropertyDrawer;
                if (this != previous_drawer)
                {
                    _PreviouslySetFieldValue = previous_drawer._PreviouslySetFieldValue;
                    Undo.undoRedoPerformed -= previous_drawer.HandleUndoRedoPerformed;
                    Undo.undoRedoPerformed += HandleUndoRedoPerformed;
                    AutoPropertyDrawerBindings[key] = new WeakReference(this);
                }
            }
            else
            {
                Undo.undoRedoPerformed += HandleUndoRedoPerformed;
                AutoPropertyDrawerBindings[key] = new WeakReference(this);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private static Dictionary<PropertyInfoBinding, WeakReference> AutoPropertyDrawerBindings =
            new Dictionary<PropertyInfoBinding, WeakReference>();
        public AutoPropertyDrawer() :
            base()
        {
        }

        private void HandleUndoRedoPerformed()
        {
            if (_TargetWR.IsAlive)
            {
                // Get the target object by its weak reference
                var target = (UnityEngine.Object)_TargetWR.Target;
                // Get the value of the field before modifying
                var field_value = fieldInfo.GetValue(target);
                // If the field value changed
                if (field_value != _PreviouslySetFieldValue)
                {
                    var serialized_object = new SerializedObject(target);
                    // Update the serialized object but don't re-affect undo in this undo handler
                    _PreviouslySetFieldValue = UpdatePropertyFromField(fieldInfo, _PropertyInfo, serialized_object, _PreviouslySetFieldValue, false);
                }
            }
            else
            {
                Undo.undoRedoPerformed -= HandleUndoRedoPerformed;
            }
        }
    }
}
