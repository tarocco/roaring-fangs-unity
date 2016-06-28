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
using UnityEditor.AnimatedValues;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RoaringFangs.Utility;

namespace RoaringFangs.Animation.Editor
{
    [CustomEditor(typeof(ControlManager))]
    [Serializable]
    public class ControlManagerEditor : UnityEditor.Editor
    {
        private AnimBool _ShowTargetGroups;
        private GUIStyle _ShowTargetGroupsFoldoutStyle;
        private Dictionary<UnityEngine.Object, AnimBool> _ExpandedObjects;

        private Dictionary<ITargetGroup, UnityEditor.Editor> _TargetGroupEditors =
            new Dictionary<ITargetGroup, UnityEditor.Editor>();
        public void OnEnable()
        {
            ControlManager self = (ControlManager)target;
            _ShowTargetGroups = new AnimBool(self.Editor__ShowTargetGroups, Repaint);
            _ExpandedObjects = self.Editor__TargetGroupsShown
                .Where(o => o != null)
                .Select(o => o as UnityEngine.Object)
                .ToDictionary(g => g, g => new AnimBool(true, DirtyAndRepaint));

        }

        private bool _DirtyTargetGroupsShown;

        private HashSet<Transform> _Include;

        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject);
            ControlManager self = (ControlManager)target;
            serializedObject.Update();
            self.SubjectPath = EditorGUILayout.DelayedTextField("Subject Path", self.SubjectPath);
            self.Subject = (GameObject)EditorGUILayout.ObjectField("Subject", self.Subject, typeof(GameObject), true);

            if (_Include == null)
            {
                var target_groups = TransformUtils.GetComponentsInDescendants<ITargetGroup>(self.transform, true).ToArray();
                var target_groups_ancestors_enum = TransformUtils.Ancestors(target_groups.Select(g => (g as Component).transform));
                _Include = new HashSet<Transform>(target_groups_ancestors_enum);
            }

            bool? expand_collapse_all;
            using (var horiz1 = new EditorGUILayout.HorizontalScope())
            {
                _ShowTargetGroupsFoldoutStyle = new GUIStyle(EditorStyles.foldout);
                _ShowTargetGroups.target = EditorGUILayout.Foldout(_ShowTargetGroups.target, "Show Target Groups");
                if (GUILayout.Button("▼", GUILayout.Width(48f)))
                    expand_collapse_all = true;
                else if (GUILayout.Button("▲", GUILayout.Width(48f)))
                    expand_collapse_all = false;
                else
                    expand_collapse_all = null;
                bool do_expand = expand_collapse_all.HasValue && expand_collapse_all.Value;
                _ShowTargetGroups.target |= do_expand;
                self.Editor__ShowTargetGroups = _ShowTargetGroups.target;
                if (expand_collapse_all.HasValue)
                {
                    foreach (var t in _Include)
                    {
                        AnimBool show_children;
                        if (!_ExpandedObjects.TryGetValue(t.gameObject, out show_children))
                            show_children = new AnimBool(!expand_collapse_all.Value, DirtyAndRepaint);
                        show_children.target = expand_collapse_all.Value;
                        _ExpandedObjects[t.gameObject] = show_children;
                    }
                }
            }
            using (var _fade1 = new EditorGUILayout.FadeGroupScope(_ShowTargetGroups.faded))
            {
                if (_fade1.visible)
                {
                    DrawTargetGroupTree(_Include, self.transform, 0, ref _DirtyTargetGroupsShown);
                }
            }
            if (_DirtyTargetGroupsShown)
            {
                self.Editor__TargetGroupsShown = _ExpandedObjects
                    .Where(e => e.Value.value)
                    .Select(e => e.Key as GameObject).ToArray();
                _DirtyTargetGroupsShown = false;
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTargetGroupTree(HashSet<Transform> include, Transform current, int depth, ref bool dirty)
        {
            var children = TransformUtils.GetChildren(current);
            foreach(var child in children)
            {
                _DrawTargetGroupTree(include, child, depth, ref dirty);
            }
        }

        private void _DrawTargetGroupTree(HashSet<Transform> include, Transform current, int depth, ref bool dirty)
        {
            if (!include.Contains(current))
                return;

            AnimBool show_children;
            var current_game_object = current.gameObject;
            var current_target_group = current.GetComponent<ITargetGroup>();

            if (!_ExpandedObjects.TryGetValue(current_game_object, out show_children))
            {
                show_children = new AnimBool(false, DirtyAndRepaint);
                _ExpandedObjects[current_game_object] = show_children;
                _DirtyTargetGroupsShown = true;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                float toggle_width = EditorGUIUtility.singleLineHeight;
                if (current_target_group != null)
                    current_target_group.Active = EditorGUILayout.Toggle(current_target_group.Active, GUILayout.Width(toggle_width));
                else
                    current_game_object.SetActive(EditorGUILayout.Toggle(current_game_object.activeSelf, GUILayout.Width(toggle_width)));
                GUILayout.Space(0.5f * toggle_width);
                show_children.target = EditorGUILayout.Foldout(show_children.target, current_game_object.name);
            }

            //bool gui_enabled_at_start = UnityEngine.GUI.enabled;
            //UnityEngine.GUI.enabled &= current_game_object.activeSelf;
            EditorGUI.indentLevel++;
            if (EditorGUILayout.BeginFadeGroup(show_children.faded))
            {
                UnityEditor.Editor editor;
                if (current_target_group != null)
                {
                    if (!_TargetGroupEditors.TryGetValue(current_target_group, out editor))
                    {
                        editor = CreateEditor(current_target_group as Component);
                        _TargetGroupEditors[current_target_group] = editor;
                    }
                    DrawPropertiesExcluding(editor.serializedObject, "m_Script");
                }
                
                foreach (Transform t in current)
                {
                    _DrawTargetGroupTree(include, t, depth + 1, ref dirty);
                }
            }
            FixedEndFadeGroup(show_children.faded);
            EditorGUI.indentLevel--;
            //UnityEngine.GUI.enabled = gui_enabled_at_start;
        }

        private void DirtyAndRepaint()
        {
            _DirtyTargetGroupsShown = true;
            Repaint();
        }

        // http://answers.unity3d.com/questions/1096244/custom-editor-fade-group-inside-fade-group.html
        private static void FixedEndFadeGroup(float aValue)
        {
            if (aValue == 0f || aValue == 1f)
                return;
            EditorGUILayout.EndFadeGroup();
        }
    }
}