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

#if UNITY_EDITOR

using UnityEditor;

#endif

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace RoaringFangs.Attributes
{
    public class AutoPropertyAttribute : PropertyAttribute
    {
#if UNITY_EDITOR

        #region Types and Delegates

        /// <summary>
        /// A delegate type that handles drawing the PropertyField for a property
        /// </summary>
        public delegate bool PropertyFieldHandler(Rect position, SerializedProperty property, GUIContent label);

        private struct PropertyInfoBindingKey
        {
            public PropertyInfo PropertyInfo;
            public int TargetId;
        }

        private class PropertyInfoBindingValue
        {
            public WeakReference AutoPropertyAttributeWR;
            public WeakReference TargetWR;
        }

        #endregion Types and Delegates

        #region Instance Fields/Properties

        private bool _Delayed;

        /// <summary>
        /// Use delayed input for this property field. Compare to <seealso cref="DelayedAttribute"/>.
        /// </summary>
        public bool Delayed
        {
            get { return _Delayed; }
            set { _Delayed = value; }
        }

        private FieldInfo _FieldInfo;

        public FieldInfo FieldInfo
        {
            get { return _FieldInfo; }
            set { _FieldInfo = value; }
        }

        private PropertyInfo _PropertyInfo;

        /// <summary>
        /// PropertyInfo for property associated with the field with this drawer's AutoPropertyAttribute
        /// </summary>
        public PropertyInfo PropertyInfo
        {
            get { return _PropertyInfo; }
            set { _PropertyInfo = value; }
        }

        private PropertyFieldHandler _DrawPropertyField = null;

        /// <summary>
        /// Draw method to use in OnGUI for this property
        /// </summary>
        public PropertyFieldHandler DrawPropertyField
        {
            get { return _DrawPropertyField; }
            protected set { _DrawPropertyField = value; }
        }

        #endregion Instance Fields/Properties

        #region Static Fields/Properties

        /// <summary>
        /// Characters to trim from field names when searching for corresponding properties
        /// </summary>
        private static readonly char[] _AutoPropertyTrimChars = { '_' };

        private static Dictionary<PropertyInfoBindingKey, PropertyInfoBindingValue> CachedAutoPropertyBindings =
            new Dictionary<PropertyInfoBindingKey, PropertyInfoBindingValue>();

        private const BindingFlags DefaultFlags =
            BindingFlags.GetProperty |
            BindingFlags.SetProperty |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        #endregion Static Fields/Properties

        #region Instance Methods

        /// <summary>
        /// Perform late binding on target and handlers
        /// </summary>
        public void UpdateLateBindings(UnityEngine.Object target)
        {
            // Key for binding
            var key = new PropertyInfoBindingKey()
            {
                TargetId = target.GetInstanceID(),
                PropertyInfo = PropertyInfo,
            };

            // Note: be advised that WeakReference.Target is not always the same thing as AutoPropertyAttribute._TargetWR
            // The previous AutoPropertyAttribute weak reference for this key
            PropertyInfoBindingValue previous_attribute_binding_value;
            // Whether there was another binding cached for this target's property
            bool have_previous = CachedAutoPropertyBindings.TryGetValue(key, out previous_attribute_binding_value);
            // Whether the previous AutoPropertyAttribute is still alive
            bool previous_attribute_is_alive = have_previous && previous_attribute_binding_value.AutoPropertyAttributeWR.IsAlive;
            // If the previous attribute's target for this target's property is still alive
            if (previous_attribute_is_alive)
            {
                // Get the previous attribute
                var previous_attribute = previous_attribute_binding_value.AutoPropertyAttributeWR.Target as AutoPropertyAttribute;
                // If this is not the previous AutoPropertyAttribute
                if (this != previous_attribute)
                {
                    // Update the cache entry
                    CachedAutoPropertyBindings[key].AutoPropertyAttributeWR.Target = this;
                }
            }
            else
            {
                // If the cache still has the dead AutoPropertyAttribute for the target's property,
                if (have_previous)
                    CachedAutoPropertyBindings[key].AutoPropertyAttributeWR.Target = this;
                else
                    CachedAutoPropertyBindings[key] = new PropertyInfoBindingValue()
                    {
                        AutoPropertyAttributeWR = new WeakReference(this),
                        TargetWR = new WeakReference(target),
                    };
            }
        }

        public void Validate(SerializedObject serialized_object, UnityEngine.Object target, bool with_undo)
        {
            //PreviouslySetPropertyValue = UpdatePropertyFromField(FieldInfo, PropertyInfo, serialized_object, target, with_undo, PreviouslySetPropertyValue);
            UpdatePropertyFromField(FieldInfo, PropertyInfo, serialized_object, target, with_undo);
            UpdateLateBindings(target);
        }

        #endregion Instance Methods

        #region Static Methods

        /// <summary>
        /// Gets the target's property info from field info and AutoPropertyAttribute
        /// </summary>
        public static PropertyInfo GetPropertyInfoAuto(FieldInfo field_info)
        {
            var field_name = field_info.Name;
            var field_type = field_info.FieldType;
            var field_declaring_type = field_info.DeclaringType;
            var auto_property_name = field_name.TrimStart(_AutoPropertyTrimChars);
            var property_info = field_declaring_type.GetProperty(auto_property_name, field_type);
            return property_info;
        }

        private static void UpdatePropertyFromField(
            FieldInfo field_info,
            PropertyInfo property_info,
            SerializedObject serialized_object,
            object target,
            //bool with_undo,
            //object previously_set_field_value)
            bool with_undo)
        {
            // Get the value of the field before modifying
            var field_value_before_apply = field_info.GetValue(target);
            // Whether the serialized property was directly modified
            if (with_undo)
                serialized_object.ApplyModifiedProperties();
            else
                serialized_object.ApplyModifiedPropertiesWithoutUndo();
            object previous_value = field_value_before_apply; // Restore to value right before this application
            // Get the value of the field after applying modified properties
            var current_value = field_info.GetValue(target);
            // Restore the field to its previous value so that the property setter can act on changes to the backing field
            field_info.SetValue(target, previous_value);
            // Invoke the setter of the property
            property_info.SetValue(target, current_value, null);
        }

        public static void ValidateAllCached(bool with_undo)
        {
            //Debug.Log("AutoPropertyAttribute.ValidateAllCached: CachedAutoPropertyBindings.Count = " + CachedAutoPropertyBindings.Count);
            foreach (var e in CachedAutoPropertyBindings.ToArray())
            {
                var property_attribute_wr = e.Value.AutoPropertyAttributeWR;
                var target_wr = e.Value.TargetWR;
                if (property_attribute_wr.IsAlive)
                {
                    var property_attribute = property_attribute_wr.Target as AutoPropertyAttribute;
                    var serialized_object = new SerializedObject(target_wr.Target as UnityEngine.Object);
                    property_attribute.Validate(serialized_object, serialized_object.targetObject, with_undo);
                }
                else
                {
                    CachedAutoPropertyBindings.Remove(e.Key);
                }
            }
        }

        private static void HandleUndoRedoPerformedAll()
        {
            ValidateAllCached(false);
        }

        #region Field Proxies

        public static bool DelayedIntField(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.DelayedIntField(position, property, label);
            return false;
        }

        public static bool DelayedFloatField(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.DelayedFloatField(position, property, label);
            return false;
        }

        public static bool DelayedTextField(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.DelayedTextField(position, property, label);
            return false;
        }

        public static bool RangeField(Rect position, SerializedProperty property, GUIContent label, float min, float max)
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

        public static PropertyFieldHandler RangeField(float min, float max)
        {
            return (a, b, c) => RangeField(a, b, c, min, max);
        }

        public static bool MinMaxField(Rect position, SerializedProperty property, GUIContent label, float min, float max)
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

        public static PropertyFieldHandler MinMaxField(float min, float max)
        {
            return (a, b, c) => MinMaxField(a, b, c, min, max);
        }

        public static bool PropertyFieldIncludingChildren(Rect position, SerializedProperty property, GUIContent label)
        {
            return EditorGUI.PropertyField(position, property, label, true);
        }

        #endregion Field Proxies

        /// <summary>
        /// Gets the cortrect property field drawing method for a given SerializedProperty.
        /// </summary>
        /// <param name="delayed">Whether the field should be delayed. See <seealso cref="AutoPropertyAttribute.Delayed"/>.</param>
        public static PropertyFieldHandler GetPropertyFieldDrawer(SerializedPropertyType sp_type, bool delayed)
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
        public static PropertyFieldHandler GetPropertyFieldDrawer(Type property_type, bool delayed)
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

        #endregion Static Methods

        #region Static Constructor

        static AutoPropertyAttribute()
        {
            Undo.undoRedoPerformed += HandleUndoRedoPerformedAll;
        }

        #endregion Static Constructor

#endif

        #region Instance Constructors

        public AutoPropertyAttribute() :
            base()
        {
        }

        public AutoPropertyAttribute(Type type, string property_name) :
            this()
        {
#if UNITY_EDITOR
            if (type != null)
                PropertyInfo = type.GetProperty(property_name, DefaultFlags);
#endif
        }

        #endregion Instance Constructors
    }
}