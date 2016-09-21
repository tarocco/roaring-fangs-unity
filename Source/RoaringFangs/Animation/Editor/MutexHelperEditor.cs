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
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.Animation.Editor
{
    [CustomEditor(typeof(MutexHelper))]
    public class MutexHelperEditor : UnityEditor.Editor
    {
        protected static bool DrawSelector(MutexHelper self, IActiveStateProperty[] groups)
        {
            var group_behaviors = groups
                .Cast<MonoBehaviour>()
                .ToArray();
            string[] group_names = group_behaviors
                .Select(g => g.name).ToArray();
            int index_selected = Math.Max(0, Array.IndexOf(groups, self.Selected));
            index_selected = EditorGUILayout.Popup(index_selected, group_names);
            IActiveStateProperty selected;
            if (index_selected < groups.Length)
                selected = groups[index_selected];
            else
                selected = null;
            if (self.Selected != selected)
            {
                var affected_game_objects = group_behaviors
                    .Select(g => g.gameObject).ToArray();
                Undo.RecordObjects(affected_game_objects, "Select Target Group");
                self.Selected = selected;
                foreach (var group_behavior in group_behaviors)
                    EditorUtility.SetDirty(group_behavior);
                return true;
            }
            return false;
        }

        public override void OnInspectorGUI()
        {
            _OnInspectorGUI();
        }

        protected virtual void _OnInspectorGUI(params string[] properties_to_exclude)
        {
            MutexHelper self = (MutexHelper)target;
            if (DrawSelector(self, self.Controls.ToArray()))
            {
                EditorUtility.SetDirty(self);
            }
            DrawPropertiesExcluding(serializedObject, properties_to_exclude);
        }
    }

    public class MutexHelperEditorBrief : MutexHelperEditor
    {
        public override void OnInspectorGUI()
        {
            base._OnInspectorGUI("m_Script");
        }
    }

    public class MutexHelperEditorInline : MutexHelperEditor
    {
        public override void OnInspectorGUI()
        {
            MutexHelper self = (MutexHelper)target;
            if (DrawSelector(self, self.Controls.ToArray()))
            {
                EditorUtility.SetDirty(self);
            }
        }
    }
}