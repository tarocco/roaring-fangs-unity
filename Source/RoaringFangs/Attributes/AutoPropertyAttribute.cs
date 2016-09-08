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
            public string PropertyPath;
        }

        #endregion Types and Delegates

        #region Instance Fields/Properties

        private bool _Delayed;

        /// <summary>
        /// Use delayed input for this property field.
        /// <seealso cref="DelayedAttribute"/>
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
            set
            {
                if (String.IsNullOrEmpty(_PropertyName))
                {
                    string property_name = value.Name;
                    _PropertyName = property_name.TrimStart(_AutoPropertyTrimChars);
                }
                _FieldInfo = value;
            }
        }

        private string _PropertyName;
        public string PropertyName
        {
            get { return _PropertyName; }
            private set { _PropertyName = value; }
        }

        private PropertyInfo _PropertyInfo;
        /// <summary>
        /// PropertyInfo for property associated with the field with this drawer's AutoPropertyAttribute
        /// </summary>
        public PropertyInfo PropertyInfo
        {
            get { return _PropertyInfo; }
            set
            {
                _PropertyInfo = value;
                _PropertyName = _PropertyInfo.Name;
            }
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

        public const BindingFlags DefaultPropertyBindingFlags =
            BindingFlags.Instance |
            BindingFlags.GetProperty |
            BindingFlags.SetProperty |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        public const BindingFlags DefaultFieldBindingFlags =
            BindingFlags.Instance |
            BindingFlags.GetField |
            BindingFlags.SetField |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        #endregion Static Fields/Properties

        #region Instance Methods

        /// <summary>
        /// Perform late binding on target and handlers
        /// </summary>
        public void UpdateLateBindings(UnityEngine.Object target, string property_path)
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
                        PropertyPath = property_path,
                    };
            }
        }

        public void Validate(SerializedObject serialized_object, UnityEngine.Object serialize_target, string property_path, bool with_undo)
        {
            //PreviouslySetPropertyValue = UpdatePropertyFromField(FieldInfo, PropertyInfo, serialized_object, target, with_undo, PreviouslySetPropertyValue);
            UpdatePropertyFromField(FieldInfo, PropertyInfo, serialized_object, serialize_target, property_path, with_undo);
            UpdateLateBindings(serialize_target, property_path);
        }

        #endregion Instance Methods

        #region Static Methods

        public class PropertyPath
        {
            public struct Element
            {
                private static System.Text.RegularExpressions.Regex FieldRegex =
                    new System.Text.RegularExpressions.Regex("([^\\[]+)(?:\\[(\\d+)\\])?");
                public string FieldName;
                public int? ArrayIndex;
                public Element(string field_element)
                {
                    var field_match = FieldRegex.Match(field_element);
                    var field_groups = field_match.Groups;
                    if (field_groups == null)
                        throw new Exception("field_groups is null");
                    var field_groups_count = field_groups.Count;
                    if (field_groups_count <= 1)
                        throw new Exception("field_groups did not match enough groups");
                    else
                    {
                        FieldName = field_groups[1].Value;
                        if (field_groups_count >= 2)
                        {
                            int array_index;
                            if (int.TryParse(field_groups[1].Value, out array_index))
                                ArrayIndex = array_index;
                            else
                                ArrayIndex = null;
                        }
                        else
                            ArrayIndex = null;
                    }
                }

                public object GetFieldValue(object @object, int depth)
                {
                    Type object_type = @object.GetType();
                    FieldInfo field_info;
                    while (depth > 0)
                    {
                        field_info = object_type.GetField(FieldName, DefaultFieldBindingFlags);
                        if (field_info != null)
                        {
                            object field_value = field_info.GetValue(@object);
                            if (ArrayIndex.HasValue)
                                return ((object[])field_value)[ArrayIndex.Value];
                            return field_value;
                        }
                        object_type = object_type.BaseType;
                        depth--;
                    }
                    throw new Exception("Could not get field value.");
                }


            }
            public static IEnumerable<Element> Parse(string property_path)
            {
                property_path = property_path.Replace(".Array.data[", "[");
                return property_path.Split('.').Select(e => new Element(e));
            }
        }


        private static object GetPropertyDeclaringObjectAtPath(object target, string property_path)
        {
            var path_elements = PropertyPath.Parse(property_path);
            var path_elements_except_last = path_elements.Take(path_elements.Count() - 1);
            object field_target = target;
            foreach (var e in path_elements_except_last)
                field_target = e.GetFieldValue(field_target, 4); // Surprise! It's actually a field.
            return field_target;
        }

        private static void UpdatePropertyFromField(
            FieldInfo field_info,
            PropertyInfo property_info,
            SerializedObject serialized_object,
            object target,
            string property_path,
            //bool with_undo,
            //object previously_set_field_value)
            bool with_undo)
        {
            // Get the object with this property at the given path for this target
            var property_value_at_path = GetPropertyDeclaringObjectAtPath(target, property_path);
            // Get the value of the field before modifying
            var field_value_before_apply = field_info.GetValue(property_value_at_path);
            // Whether the serialized property was directly modified
            if (with_undo)
                serialized_object.ApplyModifiedProperties();
            else
                serialized_object.ApplyModifiedPropertiesWithoutUndo();
            object previous_value = field_value_before_apply; // Restore to value right before this application
            // Get the values of the field and property after applying modified properties
            var current_field_value = field_info.GetValue(property_value_at_path);
            var current_property_value = property_info.GetValue(property_value_at_path, null);
            // Restore the field to its previous value so that the property setter can act on changes to the backing field
            field_info.SetValue(property_value_at_path, previous_value);
            // Invoke the setter of the property
            property_info.SetValue(property_value_at_path, current_property_value, null);
        }

        public static void ValidateAllCached(bool with_undo)
        {
            //Debug.Log("AutoPropertyAttribute.ValidateAllCached: CachedAutoPropertyBindings.Count = " + CachedAutoPropertyBindings.Count);
            foreach (var e in CachedAutoPropertyBindings.ToArray())
            {
                var property_attribute_wr = e.Value.AutoPropertyAttributeWR;
                var target_wr = e.Value.TargetWR;
                var property_path = e.Value.PropertyPath;
                if (property_attribute_wr.IsAlive)
                {
                    var property_attribute = property_attribute_wr.Target as AutoPropertyAttribute;
                    var serialized_object = new SerializedObject(target_wr.Target as UnityEngine.Object);
                    property_attribute.Validate(serialized_object, serialized_object.targetObject, property_path, with_undo);
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

        private static bool RangeField(
            Rect position,
            SerializedProperty property,
            GUIContent label,
            float min,
            float max,
            string min_prop_name,
            string max_prop_name)
        {
            bool has_min_prop = !String.IsNullOrEmpty(min_prop_name);
            bool has_max_prop = !String.IsNullOrEmpty(max_prop_name);
            if (has_min_prop || has_max_prop)
            {
                var serialized_object = property.serializedObject;
                var targets = serialized_object.targetObjects;
                var property_path = property.propertyPath;
                foreach (var target in targets)
                {
                    var declaring_property = GetPropertyDeclaringObjectAtPath(target, property_path);
                    var declaring_type = declaring_property.GetType();
                    // It's a long shot
                    if (has_min_prop)
                    {
                        var min_prop = declaring_type.GetProperty(min_prop_name, DefaultPropertyBindingFlags);
                        var min_prop_value = min_prop.GetValue(declaring_property, null);
                        min = Convert.ToSingle(min_prop_value);
                    }
                    if (has_max_prop)
                    {
                        var max_prop = declaring_type.GetProperty(max_prop_name, DefaultPropertyBindingFlags);
                        var max_prop_value = max_prop.GetValue(declaring_property, null);
                        max = Convert.ToSingle(max_prop_value);
                    }
                }
            }
            return RangeField(position, property, label, min, max);
        }

        public static PropertyFieldHandler RangeField(float min, float max)
        {
            return (a, b, c) => RangeField(a, b, c,
                min, max);
        }

        public static PropertyFieldHandler RangeField(string min_prop_name, float max)
        {

            return (a, b, c) => RangeField(a, b, c,
                float.NegativeInfinity, max, min_prop_name, null);
        }

        public static PropertyFieldHandler RangeField(float min, string max_prop_name)
        {
            return (a, b, c) => RangeField(a, b, c,
                min, float.PositiveInfinity, null, max_prop_name);
        }

        public static PropertyFieldHandler RangeField(string min_prop_name, string max_prop_name)
        {
            return (a, b, c) => RangeField(a, b, c,
                float.NegativeInfinity, float.PositiveInfinity, min_prop_name, max_prop_name);
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
        /// <param name="delayed">
        /// Whether the field should be delayed (like <see cref="AutoPropertyAttribute.Delayed"/>).
        /// </param>
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
        /// <param name="delayed">
        /// Whether the field should be delayed (like <see cref="AutoPropertyAttribute.Delayed"/>).
        /// </param>
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

        public AutoPropertyAttribute(string property_name) :
            this()
        {
#if UNITY_EDITOR
            PropertyName = property_name;
#endif
        }

        #endregion Instance Constructors
    }
}
