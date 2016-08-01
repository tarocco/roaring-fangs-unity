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

using System;
using System.Reflection;

namespace RoaringFangs.Utility
{
    [Serializable]
    public struct ActionSurrogate
    {
        public string MethodName;
        public UnityEngine.Object Instance;

        public ActionSurrogate(Action action)
        {
            if (action != null)
            {
                var target = action.Target;
                if (target is UnityEngine.Object)
                    Instance = action.Target as UnityEngine.Object;
                else
                    Instance = default(UnityEngine.Object);
                MethodName = action.Method.Name;
            }
            else
            {
                MethodName = default(string);
                Instance = default(UnityEngine.Object);
            }
        }

        public ActionSurrogate(MethodInfo method_info, UnityEngine.Object instance)
        {
            Instance = instance;
            MethodName = method_info.Name;
        }

        public static ActionSurrogate FromAction(Action action)
        {
            return new ActionSurrogate(action);
        }

        public static ActionSurrogate FromAction<T1>(Action<T1> action)
        {
            return new ActionSurrogate(action.Method, action.Target as UnityEngine.Object);
        }

        public static ActionSurrogate FromAction<T1, T2>(Action<T1, T2> action)
        {
            return new ActionSurrogate(action.Method, action.Target as UnityEngine.Object);
        }

        public static ActionSurrogate FromAction<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            return new ActionSurrogate(action.Method, action.Target as UnityEngine.Object);
        }

        public static ActionSurrogate FromAction<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        {
            return new ActionSurrogate(action.Method, action.Target as UnityEngine.Object);
        }

        public Action ToAction()
        {
            try
            {
                return (Action)Delegate.CreateDelegate(typeof(Action), Instance, MethodName);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public Action<T1> ToAction<T1>()
        {
            try
            {
                return (Action<T1>)Delegate.CreateDelegate(typeof(Action<T1>), Instance, MethodName);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public Action<T1, T2> ToAction<T1, T2>()
        {
            try
            {
                return (Action<T1, T2>)Delegate.CreateDelegate(typeof(Action<T1, T2>), Instance, MethodName);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public Action<T1, T2, T3> ToAction<T1, T2, T3>()
        {
            try
            {
                return (Action<T1, T2, T3>)Delegate.CreateDelegate(typeof(Action<T1, T2, T3>), Instance, MethodName);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public Action<T1, T2, T3, T4> ToAction<T1, T2, T3, T4>()
        {
            try
            {
                return (Action<T1, T2, T3, T4>)Delegate.CreateDelegate(typeof(Action<T1, T2, T3, T4>), Instance, MethodName);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static implicit operator Action(ActionSurrogate surrogate)
        {
            return surrogate.ToAction();
        }

        public static implicit operator ActionSurrogate(Action action)
        {
            return new ActionSurrogate(action);
        }
    }
}
