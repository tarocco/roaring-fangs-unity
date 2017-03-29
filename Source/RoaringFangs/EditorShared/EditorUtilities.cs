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

using RoaringFangs.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace RoaringFangs.Editor
{
#if UNITY_EDITOR

    [InitializeOnLoad]
#endif
    internal static class EditorUtilities
    {
        public static void SetDirty(UnityEngine.Object target)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(target);
#endif
        }

        public static bool AreGizmosVisible()
        {
#if UNITY_EDITOR
            var asm = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = asm.GetType("UnityEditor.GameView");
            if (type != null)
            {
                var window = EditorWindow.GetWindow(type);
                var gizmosField = type.GetField("m_Gizmos", BindingFlags.NonPublic | BindingFlags.Instance);
                if (gizmosField != null)
                    return (bool)gizmosField.GetValue(window);
            }
#endif
            return false;
        }

        public const BindingFlags PublicNonPublicInstanceFlags =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance;

        private static void DefaultMember<T>(T self, FieldInfo field_info)
        {
            var property_type = field_info.FieldType;
            object default_value;
            if (property_type.IsValueType)
                default_value = Activator.CreateInstance(property_type);
            else
                default_value = null;
            field_info.SetValue(self, default_value);
        }

        private static void DefaultMember<T>(T self, PropertyInfo property_info)
        {
            var property_type = property_info.PropertyType;
            object default_value;
            if (property_type.IsValueType)
                default_value = Activator.CreateInstance(property_type);
            else
                default_value = null;
            property_info.SetValue(self, default_value, null);
        }

        /// <summary>
        /// Used for the key type in <see cref="MemberHashes"/>
        /// </summary>
        private struct MemberBinding
        {
            private WeakReference _Object;

            public object Object
            {
                get { return _Object.Target; }
            }

            private WeakReference _MemberInfo;

            public MemberInfo MemberInfo
            {
                get { return _MemberInfo.Target as MemberInfo; }
            }

            private int _HashCode;

            public MemberBinding(object @object, MemberInfo info)
            {
                _Object = new WeakReference(@object);
                _MemberInfo = new WeakReference(info);
                _HashCode = 0; // CS0188
                if (@object is UnityEngine.Object)
                    _HashCode = (@object as UnityEngine.Object).GetInstanceID() ^ MemberInfo.GetHashCode();
                else
                    _HashCode = @object.GetHashCode() ^ MemberInfo.GetHashCode();
            }

            public override int GetHashCode()
            {
                return _HashCode;
            }

            public override bool Equals(object obj)
            {
                if (obj is MemberBinding)
                {
                    var other = (MemberBinding)obj;
                    return
                        Equals(Object, other.Object) &&
                        Equals(MemberInfo, other.MemberInfo);
                }
                return false;
            }
        }

        private static Dictionary<MemberBinding, int> MemberValueHashCache = new Dictionary<MemberBinding, int>();

        private static Dictionary<MemberBinding, Attribute[]> MemberAttributeCache = new Dictionary<MemberBinding, Attribute[]>();

        /// <returns>
        /// 1: True if key was already in cache and value has changed
        /// 2: true if key has been added or has changed and <param name="force_first_update"> is true
        /// </returns>
        private static bool UpdateCache<TKey, TValue>(
            Dictionary<TKey, TValue> cache,
            TKey key,
            TValue value,
            bool force_first_update = false)
        {
            TValue previous_value = default(TValue);
            bool is_cached = cache.TryGetValue(key, out previous_value);
            bool is_same = is_cached && value.Equals(previous_value);
            if (is_same)
                return false;
            cache[key] = value;
            return force_first_update || is_cached;
        }

        private static TValue UseCachedOrInsert<TKey, TValue>(
            Dictionary<TKey, TValue> cache,
            TKey key,
            TValue value)
        {
            TValue previous_value = default(TValue);
            if (cache.TryGetValue(key, out previous_value))
                return previous_value;
            cache[key] = value;
            return value;
        }

        private static TSpecific UseCachedOrInsert<TKey, TValue, TSpecific>(
            Dictionary<TKey, TValue> cache,
            TKey key,
            TSpecific value)
            where TValue : class
            where TSpecific : class
        {
            TValue previous_value = default(TValue);
            if (cache.TryGetValue(key, out previous_value))
                return previous_value as TSpecific;
            cache[key] = value as TValue;
            return value;
        }

        private static TAttribute[] UseCachedAttributesOrInsert<TAttribute>(
            MemberBinding key,
            MemberInfo member_info)
        {
            var attributes = member_info.GetCustomAttributes(false).OfType<TAttribute>();
            return UseCachedOrInsert(MemberAttributeCache, key, attributes.ToArray());
        }

        public static void OnBeforeSerializeAutoProperties<T>(
            T self,
            BindingFlags field_flags = PublicNonPublicInstanceFlags,
            BindingFlags property_flags = PublicNonPublicInstanceFlags)
        {
            if (self is UnityEngine.Object)
                OnBeforeSerializeAutoProperties(self as UnityEngine.Object, self, field_flags, property_flags);
            else
                OnBeforeSerializeAutoProperties(null as UnityEngine.Object, self, field_flags, property_flags);
        }

        private static int GetFieldValueHashCode(object @object)
        {
            if (@object is UnityEngine.Object)
            {
                var unity_object = (@object as UnityEngine.Object);
                // Null check must be performed after typecast because Unity's == and Equals() implementations are NOT polymorphic (virtual)
                if (unity_object == null)
                    return 0;
                return unity_object.GetInstanceID();
            }
            if (@object is UnityEngine.Object[])
            {
                var unity_object_array = @object as UnityEngine.Object[];
                if (unity_object_array == null)
                    return 0;
                if (!unity_object_array.Any())
                    return unity_object_array.GetHashCode();
                return unity_object_array
                    .Select(o => o == null ? 0 : o.GetInstanceID())
                    .Aggregate((a, b) => a ^ b);
            }
            if (@object == null)
                return 0;
            if (@object is object[])
            {
                var object_array = @object as object[];
                if (!object_array.Any())
                    return object_array.GetHashCode();
                return object_array
                    .Select(o => o.GetHashCode())
                    .Aggregate((a, b) => a ^ b);
            }
            return @object.GetHashCode();
        }

        public static void OnBeforeSerializeAutoProperties<TRoot, TSelf>(
            TRoot root,
            TSelf self,
            BindingFlags field_flags = PublicNonPublicInstanceFlags,
            BindingFlags property_flags = PublicNonPublicInstanceFlags) where TRoot : UnityEngine.Object
        {
#if UNITY_EDITOR

            // VBG 2017-02-26: This code looks really gross but it seems to be doing exactly what I want it to

            // Don't apply changes while changing play mode (Unity doesn't like it!)
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                return;

            // TODO: simplify these statements somehow?
            // Gets a lookup table for self's fields to their attributes, cached so that there is persistence until Unity changes play states instead of simply creating new ones
            var member_type = typeof(TSelf);
            var fields_to_process = member_type
                .GetFields(field_flags)
                .SelectMany(
                    f => UseCachedAttributesOrInsert<AutoPropertyAttribute>(new MemberBinding(self, f), f),
                    (f, a) => new KeyValuePair<FieldInfo, AutoPropertyAttribute>(f, a))
                .ToArray()
                .ToLookup(e => e.Key, e => e.Value);
            var properties_to_process = typeof(TSelf)
                .GetProperties(property_flags)
                .SelectMany(
                    p => UseCachedAttributesOrInsert<AutoPropertyAttribute>(new MemberBinding(self, p), p),
                    (p, a) => new KeyValuePair<PropertyInfo, AutoPropertyAttribute>(p, a))
                .ToArray()
                .ToLookup(e => e.Key, e => e.Value);

            // Work over fields
            foreach (var group in fields_to_process)
            {
                var field_info = group.Key;
                var field_value = field_info.GetValue(self);
                var field_value_hash = field_value != null ? GetFieldValueHashCode(field_value) : 0;
                // Skip fields that have not changed
                if (!UpdateCache(MemberValueHashCache, new MemberBinding(self, field_info), field_value_hash))
                    continue;

                // Make sure the declaring object is marked as dirty in case a property accessor
                // affected it from a separate call to here (just trust me on this one)
                if (root)
                    EditorUtility.SetDirty(root);

                foreach (var attribute in group) // Usually just a group of 1
                {
                    // Get the property name by removing the leading underscore
                    string property_name;
                    if (string.IsNullOrEmpty(attribute.CorrespondingMemberName))
                        property_name = field_info.Name.Substring(1);
                    else
                        property_name = attribute.CorrespondingMemberName;
                    var property_info = typeof(TSelf).GetProperty(property_name, property_flags);
                    if (property_info == null)
                        throw new ArgumentException("Cannot find corresponding property \"" + property_name + "\" for field \"" + field_info.Name + "\"");

                    // If the value is a GameObject or Component and the property type isn't, try to find the first matching component
                    //var field_value = field_info.GetValue(self);
                    //if (!property_info.PropertyType.IsSubclassOf(typeof(GameObject)))
                    //{
                    //    if (field_value is GameObject)
                    //        field_value = (field_value as GameObject).GetComponents(property_info.PropertyType).First(); // Use the first matching component of the game object
                    //    else if (field_value is Component)
                    //        field_value = (field_value as Component).GetComponents(property_info.PropertyType).First(); // Use the first matching component of the passed component's game object
                    //}

                    try
                    {
                        // We are always assuming there is a getter and setter
                        var property_value = property_info.GetValue(self, null);

                        try
                        {
                            property_info.SetValue(self, property_value, null);
                        }
                        catch (Exception ex_set)
                        {
                            Debug.LogWarning("Could not set property \"" + property_name + "\"");
                            throw ex_set;
                        }
                    }
                    catch (Exception ex_get)
                    {
                        Debug.LogWarning("Could not get property \"" + property_name + "\"");
                        throw ex_get;
                    }
                }
            }

            // Temporarily disabled until covariance/contravariance can be figured out

            /*
            // Work over properties
            foreach (var group in properties_to_process)
            {
                var property_info = group.Key;
                var property_value = property_info.GetValue(member, null);
                var property_value_hash = property_value != null ? property_value.GetHashCode() : 0;
                // Skip properties that have not changed
				if (!UpdateCache(MemberValueHashCache, new MemberBinding(member, property_info), property_value_hash))
                    continue;

                foreach (var attribute in group) // Usually just a group of 1
                {
                    try
                    {
                        var value = property_info.GetValue(member, null);
                        // If the value is a GameObject or Component and the property type isn't, try to find a matching component
                        if (!property_info.PropertyType.IsSubclassOf(typeof(GameObject)))
                        {
                            if (value is GameObject)
                                value = (value as GameObject).GetComponents(property_info.PropertyType).First();
                            else if (value is Component)
                                value = (value as Component).GetComponents(property_info.PropertyType).First();
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        DefaultProperty(member, property_info);
                        throw new ArgumentException("Could not set value on property \"" + property_info.Name + "\"", ex);
                    }
                }
            }
            */
#endif
        }

        /// <summary>
        /// Calls the necessary overload of <see cref="TransformUtils.SyncObjectWithPath"/> for
        /// serialized <see cref="GameObject"/> and <see cref="Component"/> fields with the
        /// <see cref="StickyAttribute"/> attribute on them.
        /// </summary>
        /// <param name="self">Declaring component type</param>
        /// <param name="field_flags">Binding flags for the fields</param>
        /// <typeparam name="T">Type parameter of the declaring component</typeparam>
        public static void OnBeforeSerializeStickyFields<T>(
            T self,
            BindingFlags field_flags = PublicNonPublicInstanceFlags) where T : Behaviour
        {
#if UNITY_EDITOR
            // Don't apply changes while changing play mode (Unity doesn't like it!)
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                return;

            // TODO: simplify these statements somehow?
            // Get a lookup table of field info for self's fields to all of their (cached) sticky attributes
            var fields_to_process = typeof(T)
                .GetFields(field_flags)
                .SelectMany(
                    // Cached attributes for the same bound members have priority over new ones
                    f => UseCachedAttributesOrInsert<StickyInEditorAttribute>(new MemberBinding(self, f), f),
                    (f, a) => new KeyValuePair<FieldInfo, StickyInEditorAttribute>(f, a))
                .ToArray()
                .ToLookup(e => e.Key, e => e.Value);

            // Work over fields
            foreach (var group in fields_to_process)
            {
                var field_info = group.Key;

                // See if the field is for GameObject or Component types (Transform, MonoBehaviour, etc.)
                bool field_value_inherits_gameobject = field_info.FieldType.IsSubclassOf(typeof(GameObject));
                bool field_value_inherits_component = field_info.FieldType.IsSubclassOf(typeof(Component));

                // Bail out if it's not a type we can work with
                if (!(field_value_inherits_gameobject || field_value_inherits_component))
                    throw new ArgumentException("Sticky field must be a GameObject or Component type");

                // Get the value of the field
                var field_value = field_info.GetValue(self);

                {
                    // Check to see if the non-polymorphic field value is null on its own
                    bool field_value_is_naively_null = field_value == null;

                    // Enforce "real" null values because == operator is not virtual on these!
                    if (field_value_inherits_gameobject && (field_value as GameObject) == null)
                        field_value = null;

                    if (field_value_inherits_component && (field_value as Component) == null)
                        field_value = null;
                }

                bool field_value_is_null = field_value == null;

                var field_value_hash = !field_value_is_null ? field_value.GetHashCode() : 0;

                // Skip fields that have not changed
                bool field_value_has_changed = UpdateCache(
                    MemberValueHashCache,
                    new MemberBinding(self, field_info),
                    field_value_hash,
                    true);

                if (!field_value_has_changed && !field_value_is_null)
                    continue;

                // Make sure the object is marked as dirty in case a property accessor
                // affected self from a separate call to here (just trust me on this one)
                EditorUtility.SetDirty(self);

                foreach (var attribute in group) // Usually just a group of 1
                {
                    var path = attribute.Path;
                    // Handling is slightly different for GameObject and Component types, where Component needs the specific type from the field info when calling GetComponent()
                    if (field_value_inherits_gameobject)
                    {
                        var game_object = field_value as GameObject;
                        RoaringFangs.Utility.TransformUtils.SyncObjectWithPath(self.transform, ref game_object, ref path);
                        field_info.SetValue(self, game_object);
                    }
                    else if (field_value_inherits_component)
                    {
                        var component = field_value as Component;
                        RoaringFangs.Utility.TransformUtils.SyncObjectWithPath(self.transform, ref component, ref path, field_info.FieldType);
                        field_info.SetValue(self, component);
                    }
                    // Update the path value on the StickyAttribute
                    attribute.Path = path;
                    //Debug.Log("Cache size = " + MemberAttributeCache.Count + " rows");
                }
            }
#endif
        }

#if UNITY_EDITOR

        private static void HandlePlaymodeStateChanged()
        {
            // Don't let this get too big!
            MemberValueHashCache.Clear();
            MemberAttributeCache.Clear();
        }

        static EditorUtilities()
        {
            EditorApplication.playmodeStateChanged += HandlePlaymodeStateChanged;
        }

#endif
    }
}