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

        private static void DefaultProperty<T>(T self, PropertyInfo property_info)
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
        private struct HashedMemberBinding
        {
            /// <summary>
            /// Unique id of the declaring object (hash code or instance id)
            /// </summary>
            public readonly int ObjectId;
            /// <summary>
            /// Hash code of the member info
            /// </summary>
            public readonly int InfoHash;
            public HashedMemberBinding(int object_id, int info_hash)
            {
                ObjectId = object_id;
                InfoHash = info_hash;
            }
        }
        
        /// <summary>
        /// Dictionary of cached fields/properties and hashes of their values.
        /// Compared against updated hashes to detect changes.
        /// </summary>
        private static Dictionary<HashedMemberBinding, int> MemberHashes = new Dictionary<HashedMemberBinding, int>();

        /// <summary>
        /// Updates a member value hash cache with a <see cref="HashedMemberBinding"/> key and integer hash codes or instance ids of the member values.
        /// </summary>
        /// <param name="cache">The cache to be updated</param>
        /// <param name="member_binding">Binding key value in cache</param>
        /// <param name="current_member_value_hash">Hash of the member's value</param>
        /// <returns>True if binding was already in cache and value hash has changed</returns>
        private static bool UpdateMemberValueHashCache(Dictionary<HashedMemberBinding, int> cache, HashedMemberBinding member_binding, int current_member_value_hash)
        {
            int previous_member_value_hash;
            bool property_is_cached = cache.TryGetValue(member_binding, out previous_member_value_hash);
            bool property_is_same = property_is_cached && current_member_value_hash == previous_member_value_hash;
            if (property_is_same)
                return false;
            // Update the cache
            cache[member_binding] = current_member_value_hash;
            return property_is_cached;
        }

        public static void OnBeforeSerializeAutoProperties<T>(
            T self,
            BindingFlags field_flags = PublicNonPublicInstanceFlags,
            BindingFlags property_flags = PublicNonPublicInstanceFlags) where T : Behaviour
        {
#if UNITY_EDITOR

            // VBG 2017-02-26: This code looks really gross but it seems to be doing exactly what I want it to 

            // Don't apply changes while changing play mode (Unity doesn't like it!)
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                return;

            // TODO: simplify these statements somehow?
            var fields_to_process = typeof(T)
                .GetFields(field_flags)
                .SelectMany(
                    f => f.GetCustomAttributes(false).OfType<AutoPropertyAttribute>(),
                    (f, a) => new KeyValuePair<FieldInfo, AutoPropertyAttribute>(f, a))
                .ToArray()
                .ToLookup(e => e.Key, e => e.Value);
            var properties_to_process = typeof(T)
                .GetProperties(property_flags)
                .SelectMany(
                    p => p.GetCustomAttributes(false).OfType<AutoPropertyAttribute>(),
                    (p, a) => new KeyValuePair<PropertyInfo, AutoPropertyAttribute>(p, a))
                .ToArray()
                .ToLookup(e => e.Key, e => e.Value);

            // Work over fields
            foreach (var group in fields_to_process)
            {
                var field_info = group.Key;
                var field_value = field_info.GetValue(self);
                var field_value_hash = field_value != null ? field_value.GetHashCode() : 0;
                // Skip fields that have not changed
                if (!UpdateMemberValueHashCache(MemberHashes, new HashedMemberBinding(self.GetInstanceID(), field_info.GetHashCode()), field_value_hash))
                    continue;

                // Make sure the object is marked as dirty in case a property accessor
                // affected self from a separate call to here (just trust me on this one)
                EditorUtility.SetDirty(self);

                foreach (var attribute in group) // Usually just a group of 1
                {
                    // Get the property name by removing the leading underscore
                    string property_name;
                    if (string.IsNullOrEmpty(attribute.CorrespondingMemberName))
                        property_name = field_info.Name.Substring(1);
                    else
                        property_name = attribute.CorrespondingMemberName;
                    var property_info = typeof(T).GetProperty(property_name, property_flags);
                    if (property_info == null)
                        throw new ArgumentException("Cannot find corresponding property \"" + property_name + "\" for field \"" + field_info.Name + "\"");
                    try
                    {
                        var value = field_info.GetValue(self);
                        // If the value is a GameObject or Component and the property type isn't, try to find the first matching component
                        if (!property_info.PropertyType.IsSubclassOf(typeof(GameObject)))
                        {
                            if (value is GameObject)
                                value = (value as GameObject).GetComponents(property_info.PropertyType).First();
                            else if (value is Component)
                                value = (value as Component).GetComponents(property_info.PropertyType).First();
                        }
                        property_info.SetValue(self, value, null);
                    }
                    catch (Exception ex)
                    {
                        DefaultProperty(self, property_info);
                        Debug.LogError("Could not set value on property \"" + property_name + "\"");
                        throw ex;
                    }
                }
            }

            // Work over properties
            foreach (var group in properties_to_process)
            {
                var property_info = group.Key;
                var property_value = property_info.GetValue(self, null);
                var property_value_hash = property_value != null ? property_value.GetHashCode() : 0;
                // Skip properties that have not changed
                if (!UpdateMemberValueHashCache(MemberHashes, new HashedMemberBinding(self.GetInstanceID(), property_info.GetHashCode()), property_value_hash))
                    continue;

                foreach (var attribute in group) // Usually just a group of 1
                {
                    try
                    {
                        var value = property_info.GetValue(self, null);
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
                        DefaultProperty(self, property_info);
                        throw new ArgumentException("Could not set value on property \"" + property_info.Name + "\"", ex);
                    }
                }
            }
        }

        private static void HandlePlaymodeStateChanged()
        {
            // Don't let this get too big!
            MemberHashes.Clear();
        }

        static EditorUtilities()
        {
            EditorApplication.playmodeStateChanged += HandlePlaymodeStateChanged;
        }

#endif
    }
}