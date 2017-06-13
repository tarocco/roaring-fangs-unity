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

using RoaringFangs.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace RoaringFangs.Animation.Editor
{
    [CustomEditor(typeof(ControlManager))]
    [Serializable]
    public class ControlManagerEditor : UnityEditor.Editor
    {
        private AnimBool _ShowTargetGroups;
        private GUIStyle _ShowTargetGroupsFoldoutStyle;
        private Dictionary<UnityEngine.Object, AnimBool> _ExpandedObjects;

        private Dictionary<Component, UnityEditor.Editor> _Editors =
            new Dictionary<Component, UnityEditor.Editor>();

        public void OnEnable()
        {
            ControlManager self = (ControlManager)target;
            _ShowTargetGroups = new AnimBool(self.Editor__ShowTargetGroups, Repaint);
            _ExpandedObjects = self.Editor__TargetGroupsShown
                .Where(o => o != null)
                .Select(o => o as UnityEngine.Object)
                .ToDictionary(g => g, g => new AnimBool(true, DirtyTargetGroupsAndRepaint));
            RoaringFangs.Editor.EditorHelper.HierarchyObjectPathChanged += HandleHierarchyObjectPathChanged;
        }

        public void OnDisable()
        {
            RoaringFangs.Editor.EditorHelper.HierarchyObjectPathChanged -= HandleHierarchyObjectPathChanged;
        }

        private bool _DirtyTargetGroupsShown;
        private bool _DirtyInclude;

        private HashSet<Transform> _Include;

        private HashSet<Transform> Include
        {
            get
            {
                if (_Include == null)
                {
                    ControlManager self = (ControlManager)target;
                    var target_groups = TransformUtils.GetComponentsInDescendants<ITargetGroup>(self.transform, true).ToArray();
                    var target_groups_ancestors_enum = TransformUtils.Ancestors(target_groups.Select(g => (g as Component).transform));
                    _Include = new HashSet<Transform>(target_groups_ancestors_enum);
                }
                return _Include;
            }
            set { _Include = value; }
        }

        private string _ControlManagerPath;

        private string ControlManagerPath
        {
            get
            {
                if (_ControlManagerPath == null)
                {
                    ControlManager self = (ControlManager)target;
                    _ControlManagerPath = TransformUtils.GetTransformPath(null, self.transform);
                }
                return _ControlManagerPath;
            }
            set { _ControlManagerPath = value; }
        }

        private bool _PathChangeHandledOnceThisUpdate = false;

        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject);
            ControlManager self = (ControlManager)target;
            serializedObject.Update();
            self.SubjectPath = EditorGUILayout.DelayedTextField("Subject Path", self.SubjectPath);
            self.Subject = (GameObject)EditorGUILayout.ObjectField("Subject", self.Subject, typeof(GameObject), true);

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
                    foreach (var t in Include)
                    {
                        AnimBool show_children;
                        if (!_ExpandedObjects.TryGetValue(t.gameObject, out show_children))
                            show_children = new AnimBool(!expand_collapse_all.Value, DirtyTargetGroupsAndRepaint);
                        show_children.target = expand_collapse_all.Value;
                        _ExpandedObjects[t.gameObject] = show_children;
                    }
                }
            }
            using (var fade = new EditorGUILayout.FadeGroupScope(_ShowTargetGroups.faded))
            {
                if (fade.visible)
                {
                    DrawTargetGroupTree(Include, self.transform, 0, true, ref _DirtyTargetGroupsShown);
                }
            }
            if (_DirtyTargetGroupsShown)
            {
                self.Editor__TargetGroupsShown = _ExpandedObjects
                    .Where(e => e.Value.value)
                    .Select(e => e.Key as GameObject).ToArray();
                _DirtyTargetGroupsShown = false;
            }
            _PathChangeHandledOnceThisUpdate = false;
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTargetGroupTree(HashSet<Transform> include, Transform current, int depth, bool enable_toggles, ref bool dirty)
        {
            var children = TransformUtils.GetChildren(current);
            foreach (var child in children)
            {
                _DrawTargetGroupTree(include, child, depth, enable_toggles, ref dirty);
            }
        }

        private void _DrawTargetGroupTree(HashSet<Transform> include, Transform current, int depth, bool enable_toggles, ref bool dirty)
        {
            if (!include.Contains(current))
                return;

            var e = Event.current;
            AnimBool show_children;
            var current_game_object = current.gameObject;
            var current_target_group = current.GetComponent<ITargetGroup>();
            var current_mutex_helper = current.GetComponent<MutexHelper>();
            var current_component = current_target_group as Component ?? current_mutex_helper;

            float single_line_height = EditorGUIUtility.singleLineHeight;

            if (!_ExpandedObjects.TryGetValue(current_game_object, out show_children))
            {
                show_children = new AnimBool(false, DirtyTargetGroupsAndRepaint);
                _ExpandedObjects[current_game_object] = show_children;
                _DirtyTargetGroupsShown = true;
            }

            using (var horiz1 = new EditorGUILayout.HorizontalScope())
            {
                bool gui_previously_enabled = UnityEngine.GUI.enabled;
                UnityEngine.GUI.enabled &= enable_toggles;
                if (current_target_group != null)
                    current_target_group.SetActive(EditorGUILayout.Toggle(current_target_group.ActiveSelf, GUILayout.Width(single_line_height)));
                else
                    current_game_object.SetActive(EditorGUILayout.Toggle(current_game_object.activeSelf, GUILayout.Width(single_line_height)));
                UnityEngine.GUI.enabled = gui_previously_enabled;
                GUILayout.Space(0.5f * single_line_height);
                show_children.target = EditorGUILayout.Foldout(show_children.target, current_game_object.name);
                if (!show_children.value && current_mutex_helper)
                {
                    GUILayout.Space(Mathf.Lerp(0.25f, 1f, show_children.faded) * Screen.width);
                    UnityEditor.Editor editor;
                    _Editors.TryGetValue(current_component, out editor);
                    CreateCachedEditor(current_component, typeof(MutexHelperEditorInline), ref editor);
                    _Editors[current_component] = editor;
                    editor.OnInspectorGUI();
                }

                #region Context Menu

                var last_rect = GUILayoutUtility.GetLastRect();
                if (DidMouseInRect(e, EventType.MouseUp, 1, last_rect))
                {
                    //Debug.Log("Right-clicked on: " + current_game_object.name);
                    GenericMenu right_click_menu = new GenericMenu();
                    right_click_menu.AddItem(new GUIContent("Add RegEx Target Group"), false, CreateChild<RegExTargetGroup>, current_game_object);
                    right_click_menu.AddItem(new GUIContent("Add Mutex Helper"), false, CreateChild<MutexHelper>, current_game_object);
                    right_click_menu.AddItem(new GUIContent("Remove"), false, Remove, current_game_object);
                    right_click_menu.ShowAsContext();
                    e.Use();
                }

                #endregion Context Menu
            }

            #region Contents Drawing

            //bool gui_enabled_at_start = UnityEngine.GUI.enabled;
            //UnityEngine.GUI.enabled &= current_game_object.activeSelf;
            //EditorGUI.indentLevel++;
            using (var horiz1 = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(single_line_height);
                using (var vert1 = new EditorGUILayout.VerticalScope())
                {
                    if (EditorGUILayout.BeginFadeGroup(show_children.faded))
                    {
                        if (current_component != null)
                        {
                            UnityEditor.Editor editor;
                            Type editor_type;
                            if (current_component is MutexHelper)
                            {
                                editor_type = typeof(MutexHelperEditorBrief);
                                enable_toggles = false;
                            }
                            else
                            {
                                editor_type = null;
                                enable_toggles = true;
                            }
                            _Editors.TryGetValue(current_component, out editor);
                            CreateCachedEditor(current_component, editor_type, ref editor);
                            _Editors[current_component] = editor;
                            if (editor_type != null)
                                editor.OnInspectorGUI();
                            else
                                DrawPropertiesExcluding(editor.serializedObject, "m_Script");
                        }

                        foreach (Transform t in current)
                        {
                            _DrawTargetGroupTree(include, t, depth + 1, enable_toggles, ref dirty);
                        }
                    }
                    FixedEndFadeGroup(show_children.faded);
                }
            }
            //EditorGUI.indentLevel--;
            //UnityEngine.GUI.enabled = gui_enabled_at_start;

            #endregion Contents Drawing
        }

        private void CreateChild<T>(object data) where T : class
        {
            if (data is GameObject)
                Selection.activeGameObject = data as GameObject;
            var type = typeof(T);
            if (type == typeof(RegExTargetGroup))
                RegExTargetGroup.Create();
            else if (type == typeof(MutexHelper))
            {
                //MutexHelper.Create();
                RoaringFangs.Editor.MenuItems.CreateMutexHelper();
            }
            _Include = null;
        }

        private void Remove(object data)
        {
            if (data is GameObject)
                Undo.DestroyObjectImmediate(data as GameObject);
            _Include = null;
        }

        private static bool DidMouseInRect(Event e, EventType event_type, int button, Rect last_rect)
        {
            return e.type == event_type && e.button == button && last_rect.Contains(e.mousePosition);
        }

        private void DirtyTargetGroupsAndRepaint()
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

        private void HandleHierarchyObjectPathChanged(object sender, RoaringFangs.Editor.EditorHelper.HierarchyObjectPathChangedEventArgs args)
        {
            if (_PathChangeHandledOnceThisUpdate)
                return;
            bool descendants_affected =
                Paths.IsSubPath(ControlManagerPath, args.OldPath) ||
                Paths.IsSubPath(ControlManagerPath, args.NewPath);
            if (descendants_affected)
            {
                _Include = null;
                _PathChangeHandledOnceThisUpdate = true;
            }
        }
    }
}