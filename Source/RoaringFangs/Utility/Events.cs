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

using UnityEngine.Events;
using System;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor.Events;

#endif

namespace RoaringFangs.Utility
{
    public static class Events
    {
        private static readonly string MessageWarnFallbackAdd =
@"Attempted to add event listener with non-UnityEngine.Object target to UnityEvent.
AddListenerAuto will use non-persistent fallback.";

        private static readonly string MessageWarnFallbackRemove =
@"Attempted to remove event listener with non-UnityEngine.Object target to UnityEvent.
AddListenerAuto will use non-persistent fallback for.";

        #region Add

        public static void AddListenerAuto(this UnityEvent @event, UnityAction listener)
        {
#if UNITY_EDITOR
            try
            {
                UnityEventTools.AddPersistentListener(@event, listener);
            }
            catch(ArgumentException)
            {
                Debug.LogWarning(MessageWarnFallbackAdd);
                @event.AddListener(listener);
            }
#else
            @event.AddListener(listener);
#endif
        }

        public static void AddListenerAuto<T0>(this UnityEvent<T0> @event, UnityAction<T0> listener)
        {
#if UNITY_EDITOR
            try
            {
                UnityEventTools.AddPersistentListener(@event, listener);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning(MessageWarnFallbackAdd);
                @event.AddListener(listener);
            }
#else
            @event.AddListener(listener);
#endif
        }

        public static void AddListenerAuto<T0, T1>(this UnityEvent<T0, T1> @event, UnityAction<T0, T1> listener)
        {
#if UNITY_EDITOR
            try
            {
                UnityEventTools.AddPersistentListener(@event, listener);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning(MessageWarnFallbackAdd);
                @event.AddListener(listener);
            }
#else
            @event.AddListener(listener);
#endif
        }

        public static void AddListenerAuto<T0, T1, T2>(this UnityEvent<T0, T1, T2> @event, UnityAction<T0, T1, T2> listener)
        {
#if UNITY_EDITOR
            try
            {
                UnityEventTools.AddPersistentListener(@event, listener);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning(MessageWarnFallbackAdd);
                @event.AddListener(listener);
            }
#else
            @event.AddListener(listener);
#endif
        }

        public static void AddListenerAuto<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> @event, UnityAction<T0, T1, T2, T3> listener)
        {
#if UNITY_EDITOR
            try
            {
                UnityEventTools.AddPersistentListener(@event, listener);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning(MessageWarnFallbackAdd);
                @event.AddListener(listener);
            }
#else
            @event.AddListener(listener);
#endif
        }

        #endregion Add

        #region Remove

        public static void RemoveListenerAuto(this UnityEvent @event, UnityAction listener)
        {
#if UNITY_EDITOR
            try
            {
                UnityEventTools.RemovePersistentListener(@event, listener);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning(MessageWarnFallbackRemove);
                @event.RemoveListener(listener);
            }
#else
            @event.RemoveListener(listener);
#endif
        }

        public static void RemoveListenerAuto<T0>(this UnityEvent<T0> @event, UnityAction<T0> listener)
        {
#if UNITY_EDITOR
            try
            {
                UnityEventTools.RemovePersistentListener(@event, listener);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning(MessageWarnFallbackRemove);
                @event.RemoveListener(listener);
            }
#else
            @event.RemoveListener(listener);
#endif
        }

        public static void RemoveListenerAuto<T0, T1>(this UnityEvent<T0, T1> @event, UnityAction<T0, T1> listener)
        {
#if UNITY_EDITOR
            try
            {
                UnityEventTools.RemovePersistentListener(@event, listener);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning(MessageWarnFallbackRemove);
                @event.RemoveListener(listener);
            }
#else
            @event.RemoveListener(listener);
#endif
        }

        public static void RemoveListenerAuto<T0, T1, T2>(this UnityEvent<T0, T1, T2> @event, UnityAction<T0, T1, T2> listener)
        {
#if UNITY_EDITOR
            try
            {
                UnityEventTools.RemovePersistentListener(@event, listener);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning(MessageWarnFallbackRemove);
                @event.RemoveListener(listener);
            }
#else
            @event.RemoveListener(listener);
#endif
        }

        public static void RemoveListenerAuto<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> @event, UnityAction<T0, T1, T2, T3> listener)
        {
#if UNITY_EDITOR
            try
            {
                UnityEventTools.RemovePersistentListener(@event, listener);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning(MessageWarnFallbackRemove);
                @event.RemoveListener(listener);
            }
#else
            @event.RemoveListener(listener);
#endif
        }

        #endregion Remove

        #region Set

        public static void RemAddListenerAuto(this UnityEvent @event, UnityAction listener)
        {
            RemoveListenerAuto(@event, listener);
            AddListenerAuto(@event, listener);
        }

        public static void RemAddListenerAuto<T0>(this UnityEvent<T0> @event, UnityAction<T0> listener)
        {
            RemoveListenerAuto(@event, listener);
            AddListenerAuto(@event, listener);
        }

        public static void RemAddListenerAuto<T0, T1>(this UnityEvent<T0, T1> @event, UnityAction<T0, T1> listener)
        {
            RemoveListenerAuto(@event, listener);
            AddListenerAuto(@event, listener);
        }

        public static void RemAddListenerAuto<T0, T1, T2>(this UnityEvent<T0, T1, T2> @event, UnityAction<T0, T1, T2> listener)
        {
            RemoveListenerAuto(@event, listener);
            AddListenerAuto(@event, listener);
        }

        public static void RemAddListenerAuto<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> @event, UnityAction<T0, T1, T2, T3> listener)
        {
            RemoveListenerAuto(@event, listener);
            AddListenerAuto(@event, listener);
        }

        #endregion Set
    }
}