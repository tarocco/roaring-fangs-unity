/*
The MIT License (MIT)

Copyright (c) 2013 Banbury
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

#if UNITY_EDITOR

using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.Editor
{
    public static class AnimationWindowUtils
    {
        public static Assembly GetUnityEditorAssembly()
        {
            return typeof(AnimationUtility).Assembly;
        }

        public static Type GetUnityEditorAnimationWindowType()
        {
            return GetUnityEditorAssembly().GetType("UnityEditor.AnimationWindow");
        }

        public static object GetAnimationWindow()
        {
            Type animation_window = GetUnityEditorAnimationWindowType();
            MethodInfo mi = animation_window.GetMethod("GetAllAnimationWindows", BindingFlags.Static | BindingFlags.Public);
            IEnumerable windows = (IEnumerable)mi.Invoke(null, null);
            if (windows != null)
            {
                IEnumerator enu = windows.GetEnumerator();
                if (enu.MoveNext())
                {
                    return enu.Current;
                }
            }
            return null;
        }

        public static object GetAnimationEditor(object animation_window)
        {
            if (animation_window == null)
                throw new ArgumentNullException("Animation window cannot be null");
            Type animation_window_type = GetUnityEditorAnimationWindowType();
            FieldInfo fi = animation_window_type.GetField("m_AnimEditor", BindingFlags.NonPublic | BindingFlags.Instance);
            return fi.GetValue(animation_window);
        }

        public static object GetAnimationWindowState(object animation_window_editor)
        {
            if (animation_window_editor == null)
                throw new ArgumentNullException("Animation editor cannot be null");
            Type animation_editor_type = GetUnityEditorAssembly().GetType("UnityEditor.AnimEditor");
            FieldInfo fi = animation_editor_type.GetField("m_State", BindingFlags.NonPublic | BindingFlags.Instance);
            return fi.GetValue(animation_window_editor);
        }

        public static AnimationClip GetActiveAnimationClip(object animation_window_state)
        {
            if (animation_window_state == null)
                throw new ArgumentNullException("Animation window state cannot be null");
            Type animation_window_state_type = GetUnityEditorAssembly().GetType("UnityEditorInternal.AnimationWindowState");
            PropertyInfo pi = animation_window_state_type.GetProperty("activeAnimationClip");
            return (AnimationClip)pi.GetValue(animation_window_state, null);
        }

        public static GameObject GetRootGameObject(object animation_window_state)
        {
            if (animation_window_state == null)
                throw new ArgumentNullException("Animation window state cannot be null");
            Type animation_window_state_type = GetUnityEditorAssembly().GetType("UnityEditorInternal.AnimationWindowState");
            PropertyInfo pi = animation_window_state_type.GetProperty("activeRootGameObject");
            return (GameObject)pi.GetValue(animation_window_state, null);
        }

        public static AnimationClip GetCurrentActiveAnimationClip()
        {
            var anim_window = GetAnimationWindow();
            if (anim_window == null)
                throw new ArgumentNullException("Animation window cannot be null");
            return GetCurrentActiveAnimationClip(anim_window);
        }

        public static AnimationClip GetCurrentActiveAnimationClip(object anim_window)
        {
            var anim_editor = GetAnimationEditor(anim_window);
            var anim_window_state = GetAnimationWindowState(anim_editor);
            return GetActiveAnimationClip(anim_window_state);
        }

        public static string GetPath(this Transform current)
        {
            if (current.parent == null)
                return "/" + current.name;
            return current.parent.GetPath() + "/" + current.name;
        }

        public static string GetPath(this Component component)
        {
            return component.transform.GetPath() + "/" + component.GetType().ToString();
        }

        public static string GetPath(this GameObject gameObject)
        {
            return gameObject.transform.GetPath();
        }
    }
}

#endif