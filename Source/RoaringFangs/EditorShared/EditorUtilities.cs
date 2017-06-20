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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityObject = UnityEngine.Object; // Useful

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace RoaringFangs.Editor
{
#if UNITY_EDITOR

    /// <summary>
    /// Class containing various important Unity Editor "glue" features that
    /// greatly speed up and improve scripting
    /// </summary>
    [InitializeOnLoad]
#endif
    internal static class EditorUtilities
    {
        #region Constants

        public const BindingFlags PublicNonPublicInstanceFlags =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance;

        #endregion Constants

        #region Classes and Structs

        /// <summary>
        /// Used for the key type in member value caches
        /// </summary>
        private struct MemberBinding
        {
            private readonly WeakReference _Object;

            /// <summary>
            /// Bound object of the member
            /// </summary>
            private object Object
            {
                get { return _Object.Target; }
            }

            private readonly WeakReference _MemberInfo;

            /// <summary>
            /// Member information
            /// </summary>
            private MemberInfo MemberInfo
            {
                get { return _MemberInfo.Target as MemberInfo; }
            }

            private readonly int _HashCode;

            public MemberBinding(object @object, MemberInfo info)
            {
                _Object = new WeakReference(@object);
                _MemberInfo = new WeakReference(info);
                var unity_object = @object as UnityEngine.Object;
                if (unity_object != null)
                {
                    var member_hash_code = info.GetHashCode();
                    _HashCode = unity_object.GetInstanceID() ^ member_hash_code;
                }
                else if (@object != null)
                {
                    var member_hash_code = info.GetHashCode();
                    _HashCode = @object.GetHashCode() ^ member_hash_code;
                }
                else
                    _HashCode = 0;
            }

            public override int GetHashCode()
            {
                return _HashCode;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is MemberBinding))
                    return false;
                var other = (MemberBinding)obj;
                return
                    Equals(Object, other.Object) &&
                    Equals(MemberInfo, other.MemberInfo);
            }

            public override string ToString()
            {
                return "MemberBinding" + _HashCode;
            }
        }

        #endregion Classes and Structs

        #region Utility Functions

        /// <summary>
        /// Auto-stubbing method to set an object dirty in the Unity editor.
        /// Automatically removes editor-specific code for player builds.
        /// </summary>
        /// <param name="target">Object to set dirty</param>
        public static void SetDirty(UnityObject target)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(target);
#endif
        }

        /// <summary>
        /// Auto-stubbing method returns whether editor gizmos are being drawn.
        /// Automatically removes editor-specific code for player builds.
        /// </summary>
        /// <returns>True if gizmos are being drawn</returns>
        public static bool AreGizmosVisible()
        {
#if UNITY_EDITOR
            var asm = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = asm.GetType("UnityEditor.GameView");
            if (type == null)
                return false;
            var window = EditorWindow.GetWindow(type);
            var gizmos_field = type.GetField("m_Gizmos", BindingFlags.NonPublic | BindingFlags.Instance);
            if (gizmos_field != null)
                return (bool)gizmos_field.GetValue(window);
#endif
            return false;
        }

        #endregion Utility Functions

        #region Caching and Hashing

        /// <summary>
        /// Dictionary containing values of members (fields and/or properties)
        /// marked with <see cref="AutoPropertyAttribute"/> cached by
        /// <see cref="OnBeforeSerializeAutoProperties{TSelf}"/> when called on
        /// their declaring objects.
        /// </summary>
        private static readonly Dictionary<MemberBinding, int>
            AutoPropertyMemberValueHashCache =
            new Dictionary<MemberBinding, int>();

        private static readonly Dictionary<MemberBinding, int>
            StickyPropertyMemberValueHashCache =
            new Dictionary<MemberBinding, int>();

        private static readonly Dictionary<MemberBinding, Attribute[]>
            MemberAttributeCache =
            new Dictionary<MemberBinding, Attribute[]>();

        public static int AutoPropertyMemberValueHashCacheSize
        {
            get { return AutoPropertyMemberValueHashCache.Count; }
        }

        public static int StickyPropertyMemberValueHashCacheSize
        {
            get { return StickyPropertyMemberValueHashCache.Count; }
        }

        public static int MemberAttributeCacheSize
        {
            get { return MemberAttributeCache.Count; }
        }

        /// <summary>
        /// Updates a cache dictionary and returns true if an entry was added
        /// or if the existing cached value has been updated.
        /// </summary>
        /// <returns>
        /// 1: True if key was already in cache and value has changed
        /// 2: true if key has been added or has changed and
        /// <param name="force_first_update"> is true.
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

        private static TSpecific UseCachedOrInsert<TKey, TValue, TSpecific>(
            Dictionary<TKey, TValue> cache,
            TKey key,
            TSpecific value)
            where TValue : class
            where TSpecific : class
        {
            TValue previous_value;
            if (cache.TryGetValue(key, out previous_value))
                return previous_value as TSpecific;
            cache[key] = value as TValue;
            return value;
        }

        private static Attribute[] UseCachedAttributesOrInsert(
            MemberBinding key,
            MemberInfo member_info)
        {
            var attributes = member_info.GetCustomAttributes(false)
                .Cast<Attribute>()
                .ToArray();
            return UseCachedOrInsert(MemberAttributeCache, key, attributes);
        }

        /// <summary>
        /// Recursively gets a hash code of an object or an aggregated
        /// hash code of an array of objects.
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        private static int GetDeepHashCode(object @object)
        {
            // Check to see if the object is a UnityEngine.Object
            // Un-typed null check must be performed after this typed check because
            // Unity's == and Equals() implementations are NOT polymorphic (virtual)
            var unity_object = @object as UnityObject;
            if (unity_object != null)
                return unity_object.GetInstanceID();

            // If it's really null, return 0
            if (@object == null)
                return 0;

            // Use GetHashCode for non-enumerable objects
            var type = @object.GetType();
            if (!type.IsArray)
                return @object.GetHashCode();
            var enumerable = @object as IEnumerable;
            if (enumerable == null)
                return @object.GetHashCode();

            // If the object is actually an array of objects/structs
            int hash_code;
            var object_enumerable = enumerable
                .Cast<object>()
                .ToArray();
            if (object_enumerable.Any())
                hash_code = object_enumerable
                    .Select(GetDeepHashCode)
                    .Aggregate((a, b) => a ^ b);
            else
                hash_code = @object.GetHashCode();
            // Invert the bits on array types to avoid collisions between
            // single values and arrays with only one element
            return ~hash_code;
        }

        /// <summary>
        /// Lookup for cached attributes on instance-bound members infos of
        /// declaring class instances. Inserts instance-bound member infos into
        /// the cache
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <typeparam name="TDeclaring"></typeparam>
        /// <typeparam name="TMemberInfo"></typeparam>
        /// <param name="self"></param>
        /// <param name="binding_flags"></param>
        /// <returns></returns>
        private static ILookup<TMemberInfo, TAttribute>
            ReadThruMemberAttributeCacheLookup<TAttribute, TDeclaring, TMemberInfo>(
            TDeclaring self,
            BindingFlags binding_flags)
            where TMemberInfo : MemberInfo
            where TAttribute : Attribute
        {
            // Type object of declaring type
            var declaring_type = typeof(TDeclaring);
            return declaring_type
                // Get all of the members
                .GetMembers(binding_flags)
                // Limit the selection to the type of members we want
                .OfType<TMemberInfo>()
                // Now for each member, select its (cached) attributes or write
                // them into the cache. Unwrap the selected attributes and then
                // for each one select a key value pair of the member info and
                // its attributes.
                .SelectMany(
                    // Select the cached attributes or else write to the cache.
                    // Then limit that selection by the TAttribute type parameter
                    f => UseCachedAttributesOrInsert(new MemberBinding(self, f), f).OfType<TAttribute>(),
                    // Select KVPs of the member info and the attribute
                    (f, a) => new KeyValuePair<TMemberInfo, TAttribute>(f, a))
                // Convert the collection of the key value pairs into a lookup
                // of MemberInfo to Attributes
                .ToLookup(e => e.Key, e => e.Value);
        }

        #endregion Caching and Hashing

        /// <summary>
        /// Automatically invokes corresponding property accessors of fields
        /// marked with <see cref="AutoPropertyAttribute"/>
        /// </summary>
        /// <remarks>
        /// Call this from <see cref="ISerializationCallbackReceiver.OnBeforeSerialize"/>
        /// </remarks>
        /// <typeparam name="TSelf"></typeparam>
        /// <param name="self">The object to serialize</param>
        /// <param name="field_flags"></param>
        /// <param name="property_flags"></param>
        public static void OnBeforeSerializeAutoProperties<TSelf>(
            TSelf self,
            BindingFlags field_flags,
            BindingFlags property_flags)
        {
#if UNITY_EDITOR

            // VBG 2017-02-26: This code looks really gross but
            // it seems to be doing exactly what I want it to

            #region Preconditions

            // Don't apply changes while changing play mode. Unity doesn't like it!
            // Seriously, Unity will crash if you're not careful about this...
            if (EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isPlaying)
                return;

            #endregion Preconditions

            #region Member Lookup Table

            // Gets a lookup table for self's fields to their attributes,
            // cached so that there is persistence until Unity editor changes
            // play states instead of simply creating new ones.
            var fields_to_process =
                ReadThruMemberAttributeCacheLookup<
                    AutoPropertyAttribute,
                    TSelf,
                    FieldInfo>(self, field_flags);

            #endregion Member Lookup Table

            #region Process Fields

            // Work over the fields
            foreach (var group in fields_to_process)
            {
                var field_info = group.Key;
                var field_value = field_info.GetValue(self);
                var field_value_hash = GetDeepHashCode(field_value);
                // Skip fields that have not changed
                var binding = new MemberBinding(self, field_info);
                if (!UpdateCache(AutoPropertyMemberValueHashCache, binding, field_value_hash))
                    continue;

                // Make sure the declaring object is marked as dirty in case a
                // property accessor affected it from a separate call to here.
                // (Just trust me on this one.)
                if (self is UnityObject)
                    EditorUtility.SetDirty(self as UnityObject);

                foreach (var attribute in group) // Usually just a group of 1
                {
                    // Get the property name by removing the leading underscore.
                    // If the corresponding member name is specified,
                    // look for that one instead.
                    string property_name;
                    if (string.IsNullOrEmpty(attribute.CorrespondingMemberName))
                        property_name = field_info.Name.Substring(1);
                    else
                        property_name = attribute.CorrespondingMemberName;
                    var property_info = typeof(TSelf).GetProperty(
                        property_name,
                        property_flags);
                    if (property_info == null)
                        throw new ArgumentException(
                            "Cannot find corresponding property \"" + property_name + "\" for field \"" + field_info.Name + "\"");

                    #region Assignment Logic

                    try
                    {
                        // This assumes that the property has both a setter and a getter.
                        // Get ready to assign this bad boy.
                        object property_value;
                        try
                        {
                            // Get the property value using its getter, rather than using the field value.
                            // We do this here first because validation can be handled with the getter, but the
                            // setter could fail to convert its parameters when calling PropertyInfo.SetValue.
                            property_value = property_info.GetValue(self, null);
                        }
                        catch
                        {
                            // Some fault tolerance.
                            // Special case: attempt to find a matching
                            // component on the same GameObject
                            var behavior = field_value as MonoBehaviour;
                            if (behavior != null)
                            {
                                try
                                {
                                    var matching_component =
                                        behavior.GetComponent(
                                            property_info.PropertyType);
                                    // If no matching component could be found,
                                    // raise an exception about it
                                    if (matching_component == null)
                                        throw new InvalidOperationException(
                                            "Could not find a matching Component on the GameObject backing field value for this property.");
                                    // Set the backing field value to the
                                    // matching component
                                    field_info.SetValue(self, matching_component);
                                    // And then use the get accessor again
                                    property_value = property_info.GetValue(self, null);
                                }
                                catch
                                {
                                    Debug.LogWarning(
                                        "Exception thrown while trying to set property \"" + property_name + "\"\nCould not GET the value of the property");
                                    throw;
                                }
                            }
                            else
                            {
                                Debug.LogWarning(
                                        "Exception thrown while trying to set property \"" + property_name + "\"\nCould not GET the value of the property");
                                throw;
                            }
                        }

                        try
                        {
                            // Set the property using its setter.
                            property_info.SetValue(self, property_value, null);
                        }
                        catch
                        {
                            Debug.LogWarning("Exception thrown while trying to set property \"" + property_name + "\"\nCould not SET the value of the property");
                            throw;
                        }
                    }
                    catch
                    {
                        // Assume that assigning the backing field pushed an operation to the undo stack and we can reverse it.
                        // Use a deferred call to PerformUndo because Unity is silly.
                        AddEditorDelayedOneShotCall(Undo.PerformUndo);
                        throw;
                    }

                    #endregion Assignment Logic
                }
            }

            #endregion Process Fields

#endif
        }

        public static void OnBeforeSerializeAutoProperties<TSelf>(TSelf self)
        {
#if UNITY_EDITOR
            OnBeforeSerializeAutoProperties(
                self,
                PublicNonPublicInstanceFlags,
                PublicNonPublicInstanceFlags);
#endif
        }

        public static void OnBeforeSerializeAutoProperties<TSelf>(
            TSelf self,
            BindingFlags member_flags)
        {
#if UNITY_EDITOR
            OnBeforeSerializeAutoProperties(self, member_flags, member_flags);
#endif
        }

        /// <summary>
        /// EXPERIMENTAL:
        /// Calls the necessary overload of
        /// <see cref="TransformUtils.SyncObjectWithPath"/> for serialized
        /// <see cref="GameObject"/> and <see cref="Component"/> fields with
        /// the <see cref="StickyAttribute"/> attribute on them.
        /// </summary>
        /// <param name="self">Declaring component type</param>
        /// <param name="field_flags">Binding flags for the fields</param>
        /// <typeparam name="TSelf">
        /// Type parameter of the declaring component
        /// </typeparam>
        /// <remarks>
        /// TODO: checking whether an object was removed from the scene (stick)
        /// or if its reference was set to null (unstick)
        /// </remarks>
        public static void OnBeforeSerializeStickyFields<TSelf>(
            TSelf self,
            BindingFlags field_flags = PublicNonPublicInstanceFlags)
            where TSelf : Behaviour
        {
#if UNITY_EDITOR
            // Don't apply changes while changing play mode
            // (Unity doesn't like it!)
            if (EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isPlaying)
                return;

            // TODO: simplify these statements somehow?
            // Get a lookup table of field info for self's fields to all of
            // their (cached) sticky attributes
            var fields_to_process =
                ReadThruMemberAttributeCacheLookup<
                    StickyInEditorAttribute,
                    TSelf,
                    FieldInfo>(
                    self,
                    field_flags);

            // Work over fields
            foreach (var group in fields_to_process)
            {
                var field_info = group.Key;

                // See if the field is for GameObject or Component types
                // (Transform, MonoBehaviour, etc.)
                // Behavior will be slightly different for either one
                bool field_value_inherits_gameobject =
                    field_info.FieldType.IsSubclassOf(typeof(GameObject));
                bool field_value_inherits_component =
                    field_info.FieldType.IsSubclassOf(typeof(Component));

                // Bail out if it's not a type we can work with
                if (!(field_value_inherits_gameobject ||
                    field_value_inherits_component))
                    throw new ArgumentException(
                        "Sticky field must be a GameObject or Component type");

                // Get the value of the field
                var original_field_value = field_info.GetValue(self);
                var field_value = original_field_value;

                // Check to see if the un-typed field value is null on its own
                //bool field_value_is_naively_null = field_value == null;

                {
                    // Use "real" null values because == operator is not
                    // virtual on these!
                    if (field_value_inherits_gameobject &&
                        (field_value as GameObject) == null)
                        field_value = null;

                    if (field_value_inherits_component &&
                        (field_value as Component) == null)
                        field_value = null;
                }

                bool field_value_is_null = field_value == null;

                var field_value_hash = GetDeepHashCode(field_value);

                // Skip fields that have not changed.
                bool field_value_has_changed = UpdateCache(
                    StickyPropertyMemberValueHashCache,
                    new MemberBinding(self, field_info),
                    field_value_hash,
                    true);

                if (!field_value_has_changed && !field_value_is_null)
                    continue;

                // Make sure the object is marked as dirty in case a property
                // accessor affected self from a separate call to here.
                // (just trust me on this one)
                EditorUtility.SetDirty(self);

                foreach (var attribute in group) // Usually just a group of 1
                {
                    var path = attribute.Path;
                    // Handling is slightly different for GameObject and
                    // Component types, where Component needs the specific type
                    // from the field info when calling GetComponent()

                    if (field_value_inherits_gameobject)
                    {
                        var game_object = field_value as GameObject;
                        Utility.TransformUtils.SyncObjectWithPath(
                            self.transform,
                            ref game_object,
                            ref path);
                        // Update the value of the field
                        field_info.SetValue(self, game_object);
                        // Update the path value on the StickyAttribute
                        attribute.Path = path;
                    }
                    else if (field_value_inherits_component)
                    {
                        var component = field_value as Component;
                        Utility.TransformUtils.SyncObjectWithPath(
                            self.transform,
                            ref component,
                            ref path,
                            field_info.FieldType);
                        // Update the value of the field
                        field_info.SetValue(self, component);
                        // Update the path value on the StickyAttribute
                        attribute.Path = path;
                    }
                }
            }
#endif
        }

        #region Editor-specific

#if UNITY_EDITOR

        /// <summary>
        /// EDITOR ONLY:
        /// Adds a delayed callback function to the bubbling
        /// <see cref="EditorApplication.delayCall"/> delegate (event) that
        /// removes itself automatically after being called just once.
        /// </summary>
        public static void AddEditorDelayedOneShotCall(
            EditorApplication.CallbackFunction one_shot)
        {
            var callback = default(EditorApplication.CallbackFunction);
            callback = () =>
            {
                try
                {
                    one_shot();
                }
                finally
                {
                    EditorApplication.delayCall -= callback;
                }
            };
            EditorApplication.delayCall += callback;
        }

#else
        public static void AddEditorDelayedOneShotCall(Action one_shot)
        {
            // Stubbed
        }
#endif

#if UNITY_EDITOR

        private static void HandlePlaymodeStateChanged()
        {
            // Don't let the caches get too big!
            AutoPropertyMemberValueHashCache.Clear();
            StickyPropertyMemberValueHashCache.Clear();
            MemberAttributeCache.Clear();
        }

        static EditorUtilities()
        {
            EditorApplication.playmodeStateChanged +=
                HandlePlaymodeStateChanged;
        }

#endif

        #endregion Editor-specific
    }
}