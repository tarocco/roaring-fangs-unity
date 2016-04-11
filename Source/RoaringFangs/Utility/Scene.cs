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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace RoaringFangs.Utility
{
    public static class Scene
    {
        #region GameObject and Transform
        public enum NameSearchOption
        {
            NameEquals = 1,
            NameStartsWith,
            NameEndsWith,
            NameContains
        }
        /// <summary>
        /// Returns whether the supplied object's name matches the specified query
        /// </summary>
        /// <returns><c>true</c>, if query was matched, <c>false</c> otherwise.</returns>
        /// <param name="obj">Object to test</param>
        /// <param name="query">Query to match</param>
        /// <param name="option">Type of query</param>
        /// <typeparam name="T">Type of the object.</typeparam>
        private static bool MatchQuery<T>(T obj, string query, NameSearchOption option = NameSearchOption.NameEquals) where T : UnityEngine.Object
        {
            switch (option)
            {
                case NameSearchOption.NameStartsWith:
                    return obj.name.StartsWith(query);
                case NameSearchOption.NameEndsWith:
                    return obj.name.EndsWith(query);
                case NameSearchOption.NameContains:
                    return obj.name.Contains(query);
                case NameSearchOption.NameEquals:
                default:
                    return obj.name == query;
            }
        }
        public static IEnumerable<UnityEngine.Object> FindObject(string query, NameSearchOption option = NameSearchOption.NameEquals)
        {
            return FindObject<UnityEngine.Object>(query, option);
        }
        public static IEnumerable<T> FindObject<T>(string query, NameSearchOption option = NameSearchOption.NameEquals) where T : UnityEngine.Object
        {
            var objects = UnityEngine.Object.FindObjectsOfType<T>();
            foreach (var obj in objects)
            {
                if (MatchQuery(obj, query, option))
                    yield return obj as T;
            }
        }

        public static IEnumerable<T> FindChildren<T>(this GameObject game_object, string query, NameSearchOption option = NameSearchOption.NameEquals) where T : UnityEngine.Object
        {
            foreach (Transform transform in game_object.transform)
            {
                if (transform.gameObject is T)
                {
                    if (MatchQuery(transform.gameObject, query, option))
                        yield return transform.gameObject as T;
                }
            }
        }
        public static GameObject GetChild(this GameObject game_object, int index = 0)
        {
            return game_object.transform.GetChild(index).gameObject;
        }
        public static T GetChildComponent<T>(this GameObject game_object, int index = 0) where T : MonoBehaviour
        {
            return game_object.transform.GetChild(index).gameObject.GetComponent<T>();
        }
        public static int ChecksumChildren(this Transform parent)
        {
            int sum = 0;
            foreach (Transform transform in parent)
                sum += transform.GetHashCode();
            return sum;
        }
        #endregion
        #region Sprite
        public static Vector3 SpriteScale(this Sprite sprite)
        {
            return new Vector3(2.0f * sprite.bounds.extents.x, 2.0f * sprite.bounds.extents.y, 0.0f);
        }
        #endregion
        #region Coroutine
        public static IEnumerable CoroutineSafetyExclusive(IEnumerable enumerable, IntWrap mutex, Transform force_update_target = null)
        {
            // A, B, C
            // A = (A == C) ? B : A
            if (Interlocked.CompareExchange(ref mutex.Value, 1, 0) == 0)
            {
                IEnumerator enumerator = enumerable.GetEnumerator();
                do
                {
                    try
                    {
                        if (!enumerator.MoveNext())
                            break;
                    }
                    catch (Exception ex)
                    {
                        mutex.Value = 0;
                        throw ex;
                    }
                    yield return enumerator.Current;
                } while (true);
                yield return null;
                mutex.Value = 0;
            }
            else
            {
                throw new SynchronizationLockException("Unable to acquire mutex for exclusive coroutine.");
            }
        }

        public static void ForceSceneUpdate(Transform target)
        {
            // Poke the scene
            var p = target.localPosition;
            target.localPosition = p + new Vector3(0.00001f, 0.0f, 0.0f);
            target.localPosition = p;
        }
        #endregion
        #region Gizmos
        public static void GizmoBounds2D(Bounds b, Vector3 center_offset = default(Vector3))
        {
            Vector2 A, B, C, D;
            A = b.center + new Vector3(center_offset.x - b.extents.x, center_offset.y - b.extents.y);
            B = b.center + new Vector3(center_offset.x + b.extents.x, center_offset.y - b.extents.y);
            C = b.center + new Vector3(center_offset.x + b.extents.x, center_offset.y + b.extents.y);
            D = b.center + new Vector3(center_offset.x - b.extents.x, center_offset.y + b.extents.y);
            Gizmos.DrawLine(A, B);
            Gizmos.DrawLine(B, C);
            Gizmos.DrawLine(C, D);
            Gizmos.DrawLine(D, A);
        }
        #endregion
    }
}