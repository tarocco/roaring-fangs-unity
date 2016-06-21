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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace RoaringFangs.Editor
{
    public static class GUIHelper
    {
        public static Color Green = new Color(0.8f, 1.0f, 0.8f);
        public static Color Red = new Color(1.0f, 0.8f, 0.8f);

        private static GUIStyle _StyleButtonBold;
        public static GUIStyle StyleButtonBold
        {
            get
            {
                if (_StyleButtonBold == null)
                {
                    _StyleButtonBold = new GUIStyle(UnityEngine.GUI.skin.button);
                    _StyleButtonBold.fontStyle = FontStyle.Bold;
                }
                return _StyleButtonBold;
            }
        }

        private static GUIStyle _StyleLabelHeaderHint;
        public static GUIStyle StyleLabelHeaderHint
        {
            get
            {
                if (_StyleLabelHeaderHint == null)
                {
                    _StyleLabelHeaderHint = new GUIStyle(UnityEngine.GUI.skin.label);
                    _StyleLabelHeaderHint.fontSize = 14;
                    _StyleLabelHeaderHint.fixedHeight = 24;
                    _StyleLabelHeaderHint.fontStyle = FontStyle.Italic;
                    _StyleLabelHeaderHint.normal.textColor = Color.gray;
                }
                return _StyleLabelHeaderHint;
            }
        }

        /// <summary>
        /// Creates a visible horizontal divider in the editor window
        /// </summary>
        /// <param name="label">Text to display on the left side of the divider</param>
        private static void VisibleSeparator(string label = null)
        {
            bool has_label = label != null;
            Rect rect_box = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
            if (has_label)
            {
                var label_content = new GUIContent(label);
                var size_label = UnityEngine.GUI.skin.label.CalcSize(label_content);
                size_label.x += 5;
                var rect_label = new Rect(rect_box.position, size_label);
                var size_box = new Vector2(rect_box.size.x - size_label.x, 2);
                var pos_box = new Vector2(
                    rect_label.position.x + rect_label.size.x,
                    rect_label.y + rect_label.size.y / 2 - 1);
                rect_box = new Rect(pos_box, size_box);

                UnityEngine.GUI.Label(rect_label, label_content);
            }
            else
            {
                rect_box.height = 2;
            }
            UnityEngine.GUI.Box(rect_box, GUIContent.none);
        }
    }
}
