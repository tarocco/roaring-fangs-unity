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
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using System.Collections;

namespace RoaringFangs.Utility
{
	public static class Events
	{
		#region Add

		public static void AddListenerAuto (UnityEvent @event, UnityAction listener)
		{
#if UNITY_EDITOR
			UnityEventTools.AddPersistentListener (@event, listener);
#else
            @event.AddListener(listener);
#endif
		}

		public static void AddListenerAuto<T0> (UnityEvent<T0> @event, UnityAction<T0> listener)
		{
#if UNITY_EDITOR
			UnityEventTools.AddPersistentListener (@event, listener);
#else
            @event.AddListener(listener);
#endif
		}

		public static void AddListenerAuto<T0, T1> (UnityEvent<T0, T1> @event, UnityAction<T0, T1> listener)
		{
#if UNITY_EDITOR
			UnityEventTools.AddPersistentListener (@event, listener);
#else
            @event.AddListener(listener);
#endif
		}

		public static void AddListenerAuto<T0, T1, T2> (UnityEvent<T0, T1, T2> @event, UnityAction<T0, T1, T2> listener)
		{
#if UNITY_EDITOR
			UnityEventTools.AddPersistentListener (@event, listener);
#else
            @event.AddListener(listener);
#endif
		}

		public static void AddListenerAuto<T0, T1, T2, T3> (UnityEvent<T0, T1, T2, T3> @event, UnityAction<T0, T1, T2, T3> listener)
		{
#if UNITY_EDITOR
			UnityEventTools.AddPersistentListener (@event, listener);
#else
            @event.AddListener(listener);
#endif
		}

		#endregion

		#region Remove

		public static void RemoveListenerAuto (UnityEvent @event, UnityAction listener)
		{
#if UNITY_EDITOR
			UnityEventTools.RemovePersistentListener (@event, listener);
#else
            @event.RemoveListener(listener);
#endif
		}

		public static void RemoveListenerAuto<T0> (UnityEvent<T0> @event, UnityAction<T0> listener)
		{
#if UNITY_EDITOR
			UnityEventTools.RemovePersistentListener (@event, listener);
#else
            @event.RemoveListener(listener);
#endif
		}

		public static void RemoveListenerAuto<T0, T1> (UnityEvent<T0, T1> @event, UnityAction<T0, T1> listener)
		{
#if UNITY_EDITOR
			UnityEventTools.RemovePersistentListener (@event, listener);
#else
            @event.RemoveListener(listener);
#endif
		}

		public static void RemoveListenerAuto<T0, T1, T2> (UnityEvent<T0, T1, T2> @event, UnityAction<T0, T1, T2> listener)
		{
#if UNITY_EDITOR
			UnityEventTools.RemovePersistentListener (@event, listener);
#else
            @event.RemoveListener(listener);
#endif
		}

		public static void RemoveListenerAuto<T0, T1, T2, T3> (UnityEvent<T0, T1, T2, T3> @event, UnityAction<T0, T1, T2, T3> listener)
		{
#if UNITY_EDITOR
			UnityEventTools.RemovePersistentListener (@event, listener);
#else
            @event.RemoveListener(listener);
#endif
		}

		#endregion
	}
}