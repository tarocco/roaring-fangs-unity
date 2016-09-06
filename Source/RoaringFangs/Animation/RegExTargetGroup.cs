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

#if UNITY_EDITOR

using UnityEditor;

#endif

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;

using RoaringFangs.Utility;
using RoaringFangs.Attributes;
using RoaringFangs.Editor;

namespace RoaringFangs.Animation
{
#if UNITY_EDITOR

    [InitializeOnLoad]
#endif
    public class RegExTargetGroup : TargetGroupBase, ITargetGroup, IHasHierarchyIcons
    {
        #region Properties

        [SerializeField, AutoProperty]
        private TargetGroupMode _Mode;

        public TargetGroupMode Mode
        {
            get { return _Mode; }
            set
            {
                _Mode = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(gameObject);
#endif
            }
        }

        [SerializeField, AutoProperty(Delayed = true)]
        private string _Pattern = "^$";

        public string Pattern
        {
            get { return _Pattern; }
            set
            {
                try
                {
                    _Regex = new Regex(value);
                    _Pattern = value;
                    var descendants = GetDescendantsFromParentManager();
                    OnFindMatchingTargetsInDescendants(descendants);
                }
                catch (ArgumentException ex)
                {
                    Debug.LogWarning("Invalid regular expression pattern: " + value);
                }
            }
        }

        private Regex _Regex;

        private Regex Regex
        {
            get { return _Regex ?? (_Regex = new Regex(Pattern)); }
        }

        #endregion Properties

        #region Cached Target Transforms

        [SerializeField, HideInInspector]
        private TransformUtils.TransformD[] _Targets;

        /// <summary>
        /// Cached and serialized targets of this target group
        /// </summary>
        public IEnumerable<TransformUtils.ITransformD> Targets
        {
            get
            {
                if (_Targets != null)
                {
                    foreach (var target in _Targets)
                        yield return target;
                }
            }
            set
            {
                if (value != null)
                    _Targets = value.Select(t => new TransformUtils.TransformD(t.Transform, t.Depth)).ToArray();
                else
                    _Targets = null;
            }
        }

        #endregion Cached Target Transforms

        #region Targets

        private static IEnumerable<TransformUtils.ITransformD> FindMatchingTransformsD(
            IEnumerable<TransformUtils.ITransformDP> descendants, Regex regex)
        {
            var descendants_array = descendants.ToArray();
            foreach (var tp in descendants_array)
            {
                Match match = regex.Match(tp.Path);
                if (match.Success)
                {
                    // Work on the first group after matching the whole pattern
                    var groups = match.Groups;
                    if (groups.Count == 1)
                    {
                        yield return new TransformUtils.TransformD(tp.Transform, tp.Depth);
                    }
                    else if (groups.Count > 1)
                    {
                        var group1 = groups[1];
                        var grouped_parent_path = tp.Path.Substring(0, group1.Index + group1.Length);
                        int parent_depth = grouped_parent_path.Count(c => c == '/');
                        Transform parent = tp.Transform;
                        for (int d = tp.Depth; d > parent_depth; d--)
                            parent = parent.parent; // lol
                        yield return new TransformUtils.TransformD(parent, parent_depth);
                    }
                }
                //Debug.Log(tp.Path);
            }
        }

        private IEnumerable<TransformUtils.ITransformD> FindTargetsInDescendants(
            IEnumerable<TransformUtils.ITransformDP> subject_descendants_and_paths)
        {
            return FindMatchingTransformsD(subject_descendants_and_paths, Regex);
        }

        #endregion Targets

        #region Descendants

        private IEnumerable<TransformUtils.ITransformDP> GetDescendantsFromParentManager()
        {
            var manager = GetComponentInParent<ControlManager>();
            if (manager)
                return manager.CachedSubjectDescendantsAndPaths;
            else
                throw new NullReferenceException("RegExTargetGroup has no parent ControlManager!");
        }

        public void OnFindMatchingTargetsInDescendants(
            IEnumerable<TransformUtils.ITransformDP> subject_descendants_and_paths)
        {
            if (subject_descendants_and_paths != null)
                Targets = FindMatchingTransformsD(subject_descendants_and_paths, Regex);
            else
                Targets = null;
        }

        #endregion Descendants

#if UNITY_EDITOR

        #region Editor Menus

        private static readonly Type HierarchyWindow = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");

        [MenuItem("Roaring Fangs/Animation/RegEx Target Group", false, 0)]
        [MenuItem("GameObject/Roaring Fangs/Animation/RegEx Target Group", false, 0)]
        [MenuItem("CONTEXT/ControlManager/RegEx Target Group", false, 25)]
        public static RegExTargetGroup Create()
        {
            GameObject selected = Selection.activeGameObject;
            ControlManager manager;
            if (selected != null)
                manager = selected.GetComponentInParent<ControlManager>();
            else
                manager = null;
            if (manager == null)
                throw new Exception("You must select a control for this target group.");

            GameObject regex_target_group_object = new GameObject("New RegEx Target Group");
            Undo.RegisterCreatedObjectUndo(regex_target_group_object, "New Regex Target Group");
            RegExTargetGroup regex_target_group = regex_target_group_object.AddComponent<RegExTargetGroup>();

            Undo.SetTransformParent(regex_target_group_object.transform, selected.transform, "New RegEx Target Group");
            Selection.activeGameObject = regex_target_group_object;
            var hierarchy_window = EditorWindow.GetWindow(HierarchyWindow);
            if (hierarchy_window != null)
            {
                hierarchy_window.Focus();
                // TODO: figure out why this isn't working!
                var rename_go = HierarchyWindow.GetMethod("RenameGO", BindingFlags.NonPublic | BindingFlags.Instance);
                rename_go.Invoke(hierarchy_window, null);
            }
            return regex_target_group;
        }

        #endregion Editor Menus

        public void OnDrawHierarchyIcons(Rect icon_position)
        {
            var gui_color = UnityEngine.GUI.color;
            UnityEngine.GUI.color = gui_color * (Active ? Color.white : Color.gray);
            UnityEngine.GUI.Label(icon_position, GetIcon(Mode));
            UnityEngine.GUI.color = gui_color;
        }

#endif
    }
}