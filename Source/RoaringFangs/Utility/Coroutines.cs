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

#if UNITY_EDITOR

using UnityEditor;

#endif

using System.Collections;

namespace RoaringFangs.Utility
{
    public static class Coroutines
    {
#if UNITY_EDITOR

        private static void ForceUpdate(Transform target)
        {
            target.position += new Vector3(0f, 0f, 0.01f);
            target.position -= new Vector3(0f, 0f, 0.01f);
        }

        private static IEnumerator CoroutineWithForceUpdate(MonoBehaviour target, IEnumerator work)
        {
            EditorApplication.CallbackFunction force_update = () => ForceUpdate(target.transform);
            EditorApplication.update += force_update;
            try
            {
                while (work.MoveNext())
                    yield return work.Current;
            }
            finally
            {
                EditorApplication.update -= force_update;
            }
        }

#endif

        public static UnityEngine.Coroutine Start(MonoBehaviour target, IEnumerator work)
        {
#if UNITY_EDITOR
            return target.StartCoroutine(CoroutineWithForceUpdate(target, work));
#else
            return target.StartCoroutine(work);
#endif
        }

        /// <summary>
        /// Wraps each enumerator step of <paramref name="coroutine"/> in a try-catch statement.
        /// If the enumerator throws an exception, the exception message is logged
        /// as an error and the coroutine is aborted.
        /// </summary>
        /// <param name="coroutine">The coroutine enumerator to wrap.</param>
        /// <returns>The wrapped coroutine.</returns>
        public static IEnumerator GetSafeCoroutine(this IEnumerator coroutine)
        {
            for (;;)
            {
                try
                {
                    if (!coroutine.MoveNext() || coroutine.Current == null)
                        break;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    break;
                }
                yield return coroutine.Current;
            }
        }
    }
}