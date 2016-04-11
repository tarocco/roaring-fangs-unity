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
using System.Collections;
using System.Collections.Generic;

namespace RoaringFangs.Utility
{
    public class TransformUtils
    {
        #region Delegates
        public delegate T AddComponentDelegate<T>(GameObject game_object) where T : Component;
        public delegate void SetTransformParentDelegate(Transform t, Transform parent, bool worldPositionStays = true);
        #endregion
        #region Static Methods
        private static T AddComponent<T>(GameObject game_object) where T : Component
        {
            return game_object.AddComponent<T>();
        }
        public static IEnumerable<T> AddComponents<T>(
            IEnumerable<GameObject> game_objects,
            AddComponentDelegate<T> add_component = null)
            where T : Component
        {
            add_component = add_component ?? AddComponent<T>;
            foreach (GameObject game_object in game_objects)
                yield return add_component(game_object);
        }

        public static void SetTransformParent(Transform t, Transform parent, bool worldPositionStays)
        {
            t.SetParent(parent);
        }

        public static IEnumerable<Transform> GetAllDescendants(params Transform[] transforms)
        {
            return GetAllDescendants((IEnumerable<Transform>)transforms);
        }
        public static IEnumerable<Transform> GetAllDescendants(IEnumerable<Transform> transforms)
        {
            foreach (Transform t in transforms)
            {
                yield return t;
                foreach (Transform t2 in t)
                {
                    foreach (Transform d in GetAllDescendants(t2))
                        yield return d;
                }
            }
        }

        public static IEnumerable<Transform> GetAllChildren(IEnumerable<Transform> parents)
        {
            foreach (Transform parent in parents)
            {
                foreach (Transform child in parent)
                    yield return child;
            }
        }
        public static List<Transform> GetMatchingTransforms(Transform transform, string prefix)
        {
            List<Transform> matching = new List<Transform>();
            foreach (Transform t in GetAllDescendants(transform))
            {
                if (t.name.StartsWith(prefix))
                    matching.Add(t);
            }
            return matching;
        }

        public static IEnumerable<Transform> GetTransforms(IEnumerable<Component> components)
        {
            foreach (var @object in components)
                yield return @object.transform;
        }

        public static IEnumerable<Transform> GetTransforms(IEnumerable objects)
        {
            foreach (var @object in objects)
                yield return ((GameObject)@object).transform;
        }

        public static T GetComponentInChildrenExclusively<T>(Transform self) where T : Component
        {
            foreach (Transform t in self)
            {
                var component = t.GetComponent<T>();
                if (component != null)
                    return component;
            }
            return null;
        }

        public static IEnumerable<T> GetComponentsInChildrenExclusively<T>(Transform self) where T : Component
        {
            foreach (Transform t in self)
            {
                var components = t.GetComponents<T>();
                if (components != null)
                    foreach (T component in components)
                        yield return component;
            }
        }

        public static IEnumerable<T> GetComponents<T>(IEnumerable<Transform> transforms) where T : Component
        {
            foreach (Transform t in transforms)
            {
                var component = t.GetComponent<T>();
                if (component != null)
                    yield return component;
            }
        }

        public static IEnumerable<T> GetComponentsInDescendants<T>(Transform root, bool include_all = false) where T : Component
        {
            foreach (Transform t in root)
            {
                if (include_all || t.gameObject.activeSelf)
                {
                    T component = t.GetComponent<T>();
                    if (component != null)
                        yield return component;
                    foreach (T c in GetComponentsInDescendants<T>(t, include_all))
                        yield return c;
                }
            }
        }

        public class WithDepth<T>
        {
            public readonly T Self;
            public readonly int Depth;
            public WithDepth(T self, int depth)
            {
                Self = self;
                Depth = depth;
            }
        }
        public static IEnumerable<WithDepth<T>> GetComponentsInDescendantsWithDepth<T>(Transform root, bool include_all = false) where T : Component
        {
            return GetComponentsInDescendantsWithDepth<T>(root, include_all, 0);
        }
        public static IEnumerable<WithDepth<T>> GetComponentsInDescendantsWithDepth<T>(Transform root, bool include_all, int depth) where T : Component
        {
            foreach (Transform t in root)
            {
                if (include_all || t.gameObject.activeSelf)
                {
                    T component = t.GetComponent<T>();
                    if (component != null)
                        yield return new WithDepth<T>(component, depth);
                    foreach (T c in GetComponentsInDescendants<T>(t, include_all))
                        yield return new WithDepth<T>(c, depth + 1);
                }
            }
        }
        #endregion
    }
}
