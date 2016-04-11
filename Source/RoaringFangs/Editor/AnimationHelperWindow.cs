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

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using UGUI = UnityEngine.GUI;

namespace RoaringFangs.Editor
{
    public class AnimationHelperWindow : EditorWindow
    {
        private static GUIStyle RemarkStyle;
        [MenuItem("Window/Animation Helper")]
        public static void ShowWindow()
        {
            var self = GetWindow<AnimationHelperWindow>("Animtn Helper");
            self.minSize = new Vector2(192, 192);
            self.maxSize = new Vector2(256, 1024);
        }
        void OnGUI()
        {
            if(RemarkStyle == null)
            {
                RemarkStyle = new GUIStyle(UGUI.skin.label);
                RemarkStyle.fontStyle = FontStyle.Italic;
            }
            Left(OnGUIRoutine().GetEnumerator());
        }
        private IEnumerable<bool> OnGUIRoutine()
        {
            AnimationHelper.EnableStickyProperties = EditorGUILayout.ToggleLeft("Enable Sticky Properties", AnimationHelper.EnableStickyProperties);
            yield return false;
            bool enabled = UGUI.enabled;
            UGUI.enabled = false;
            if (GUILayout.Button("Repair Property Paths", GUILayout.MaxWidth(144)))
                OnRepairPropertyPaths();
            GUILayout.Label("Coming soon!", RemarkStyle);
            UGUI.enabled = true;
            yield return true;
        }
        private void Left(IEnumerator<bool> routine)
        {
            for (;;)
            {
                GUILayout.BeginHorizontal();
                routine.MoveNext();
                GUILayout.EndHorizontal();
                if (routine.Current)
                    break;
            }
        }
        private void Centered(IEnumerator<bool> routine)
        {
            for (;;)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                routine.MoveNext();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (routine.Current)
                    break;
            }
        }
        private void OnRepairPropertyPaths()
        {
            Debug.LogError("Repair property paths not implemented yet! Bun needs a break...");
            // TODO: Do it!
        }
    }
}
#endif