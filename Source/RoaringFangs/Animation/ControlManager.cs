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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RoaringFangs.Utility;

namespace RoaringFangs.Animation
{
    [ExecuteInEditMode]
    public class ControlManager : MonoBehaviour
    {
        #region Subject

        [HideInInspector]
        private GameObject _Subject;

        public GameObject Subject
        {
            get
            {
                // If the subject is not set
                if (_Subject == null)
                {
                    // If the subject path is set, find the subject by its path and set the backing field reference
                    if (!String.IsNullOrEmpty(_SubjectPath))
                    {

                        // Subject path is an absolute path
                        if (_SubjectPath.StartsWith("/"))
                        {
                            // Find in scene root
                            _Subject = GameObject.Find(_SubjectPath);
                        }
                        else
                        {
                            // Subject path is in parent-relative (right now only handling 1 level)
                            if (_SubjectPath.StartsWith("../"))
                            {
                                string subject_path_rel_to_parent = _SubjectPath.Substring(3);
                                if (transform.parent != null)
                                {
                                    // Find in parent transform
                                    _Subject = transform.parent.Find(subject_path_rel_to_parent).gameObject;
                                }
                                else
                                {
                                    // Find in scene root
                                    _Subject = GameObject.Find(subject_path_rel_to_parent);
                                }
                            }
                            // Subject path is relative
                            else
                            {
                                _Subject = transform.Find(_SubjectPath).gameObject;
                            }
                        }
                    }
                }
                return _Subject;
            }
            set
            {
                if (value != _Subject)
                {
                    _Subject = value;
                    if (value != null)
                    {
                        // TODO: is transform.parent a good idea here for the "root" argument?
                        // Right now this allows the subject to be a sibling, but not a parent
                        _SubjectPath = "../" + TransformUtils.GetTransformPath(transform.parent, value.transform);
                        SubjectPathAbs = TransformUtils.GetTransformPath(null, value.transform);
                    }
                    else
                    {
                        _SubjectPath = null;
                        SubjectPathAbs = null;
                    }
                    CachedSubjectDescendantsAndPaths = GetSubjectDescendants();
                    //NotifyControlGroupsOfSubjectDescendants();
                }
            }
        }

        #endregion

        #region Subject Paths

        [SerializeField, HideInInspector]
        private string _SubjectPath;

        public string SubjectPath
        {
            get { return _SubjectPath; }
            set
            {
                if (value != _SubjectPath)
                {
                    _SubjectPath = value;
                    if (String.IsNullOrEmpty(value))
                        Subject = null;
                }
            }
        }

        private string _SubjectPathAbs;

        public string SubjectPathAbs
        {
            get
            {
                if (String.IsNullOrEmpty(_SubjectPathAbs))
                {
                    var subject = Subject;
                    if (subject != null)
                        _SubjectPathAbs = TransformUtils.GetTransformPath(null, subject.transform);
                    else
                        return null;
                }
                return _SubjectPathAbs;
            }
            private set
            {
                _SubjectPathAbs = value;
            }
        }

        #endregion

        #region Cached Subject Descendants

        private TransformUtils.TransformDP[] _CachedSubjectDescendantsAndPaths;

        public IEnumerable<TransformUtils.ITransformDP> CachedSubjectDescendantsAndPaths
        {
            get
            {
                // Lazy initialization
                if (_CachedSubjectDescendantsAndPaths == null)
                {
                    CachedSubjectDescendantsAndPaths = GetSubjectDescendants();
                }
                // Double null check for quality assurance
                if (_CachedSubjectDescendantsAndPaths != null)
                {
                    return _CachedSubjectDescendantsAndPaths.Cast<TransformUtils.ITransformDP>().ToArray();
                }
                return null;
            }
            private set
            {
                // Ifsetting the cached subject descendants
                if (value != null)
                {
                    // Create a concrete struct array from the abstract (interface) enumerable
                    _CachedSubjectDescendantsAndPaths = value
                        .Select(t => new TransformUtils.TransformDP(t.Transform, t.Depth, t.Path))
                        .ToArray();
                    // Notify control groups with descendants
                    NotifyControlGroupsOfSubjectDescendants(value);
                }
                // else if clearing them and they aren't already cleared
                else if (_CachedSubjectDescendantsAndPaths != null)
                {
                    _CachedSubjectDescendantsAndPaths = null;
                    // Notify control groups to clear
                    NotifyControlGroupsOfSubjectDescendants(null);
                }

            }
        }

        public void NotifyControlGroupsOfSubjectDescendants()
        {
            NotifyControlGroupsOfSubjectDescendants(CachedSubjectDescendantsAndPaths);
        }

        private void NotifyControlGroupsOfSubjectDescendants(IEnumerable<TransformUtils.ITransformDP> cached_subject_descendants_and_paths)
        {
            var groups = TransformUtils.GetComponentsInDescendants<TargetGroupBase>(transform, true).OfType<ITargetGroup>();
            foreach (var group in groups)
            {
                group.OnFindMatchingTargetsInDescendants(cached_subject_descendants_and_paths);
            }
        }

        protected IEnumerable<TransformUtils.ITransformDP> GetSubjectDescendants()
        {
            if (Application.isPlaying)
                Debug.LogWarning("CollectSubjectDescendants called at runtime!");
            if (Subject == null)
                return null;
            return TransformUtils.GetAllDescendantsWithPaths(Subject.transform.parent, Subject.transform);
        }

        #endregion

        #region Targets

        private struct TargetInfo
        {
            public readonly int Depth;
            public TargetInfo(int depth)
            {
                Depth = depth;
            }
        }

        private struct TargetRule
        {
            public readonly Transform Transform;
            public readonly TargetInfo Info;

            public TargetRule(Transform transform, int depth)
            {
                Transform = transform;
                Info = new TargetInfo(depth);
            }
        }

        private Dictionary<Transform, TargetInfo> TargetDataPrevious = new Dictionary<Transform, TargetInfo>();

        #endregion

        void OnEnable()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                CachedSubjectDescendantsAndPaths = GetSubjectDescendants();
                RoaringFangs.Editor.EditorHelper.HierarchyObjectPathChanged += HandleHierarchyObjectPathChanged;
            }
            else
            {
                RoaringFangs.Editor.EditorHelper.HierarchyObjectPathChanged -= HandleHierarchyObjectPathChanged;
            }
#endif
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            RoaringFangs.Editor.EditorHelper.HierarchyObjectPathChanged -= HandleHierarchyObjectPathChanged;
#endif
        }

#if UNITY_EDITOR
        private void HandleHierarchyObjectPathChanged(object sender, RoaringFangs.Editor.EditorHelper.HierarchyObjectPathChangedEventArgs args)
        {
            // If the change had anything to do with the subject, notify control groups of the changes to the subject descendants
            if (!String.IsNullOrEmpty(SubjectPathAbs))
            {
                if (IsSubPath(SubjectPathAbs, args.NewPath) ||
                    IsSubPath(SubjectPathAbs, args.OldPath))
                {
                    CachedSubjectDescendantsAndPaths = GetSubjectDescendants();
                    NotifyControlGroupsOfSubjectDescendants();
                }
            }
        }

        private static bool IsSubPath(string base_path, string sub_path)
        {
            return !String.IsNullOrEmpty(sub_path) && sub_path.StartsWith(base_path);
        }
#endif

        void Update()
        {
            var groups = TransformUtils.GetComponentsInDescendants<TargetGroupBase>(transform, true).OfType<ITargetGroup>();
            // For each groups array, select valid target lists in all of the target groups and create
            // rules on whether to show or hide the targets based on the control group's active state
            IEnumerable<KeyValuePair<ITargetGroup, IEnumerable<TargetRule>>> grouped_targets = groups
                .Where(g => g.Targets != null)
                    .Select(g => new KeyValuePair<ITargetGroup, IEnumerable<TargetRule>>(g, g.Targets
                            .Select(t => new TargetRule(t.Transform, t.Depth))));
            /*
            IEnumerable<TargetRule> targets = groups
                .Where(g => g.Targets != null)
                .SelectMany(g => g.Targets
                    .Select(t => new TargetRule(t.Transform, t.Depth, (g as TargetGroupBase).gameObject.activeInHierarchy)));

            // Buffer the selection into an array
            var targets_array = targets.ToArray();
            */
            // Create a dictionary to collect valid target data to compare against in the next update
            var target_data_previous = new Dictionary<Transform, TargetInfo>();

            foreach (var grouped_target in grouped_targets)
            {
                // Get the group and targets from the grouping object
                var group = grouped_target.Key;
                var targets = grouped_target.Value;
                // Buffer the selection into an array
                var targets_array = targets.ToArray();
                foreach (var target in targets_array)
                {
                    // If the target transform is valid (not deleted)
                    if (target.Transform != null)
                    {
                        var func = TargetGroupModesFunctor.GetModeFunction(group.Mode);
                        // Active is set to mode function of current activeSelf and group active
                        bool active = func(target.Transform.gameObject.activeSelf, group.Active);
                        target.Transform.gameObject.SetActive(active);
                        // Collect valid target data
                        target_data_previous[target.Transform] = target.Info;
                    }
                }
            }
            // Replace previous value dictionary with still-valid targets
            TargetDataPrevious = target_data_previous;
        }
#if UNITY_EDITOR
        [MenuItem("Roaring Fangs/Animation/Control Manager", false, 0)]
        [MenuItem("GameObject/Roaring Fangs/Control Manager", false, 0)]
        public static ControlManager Create()
        {
            GameObject manager_object = new GameObject("Control Manager");
            Undo.RegisterCreatedObjectUndo(manager_object, "Add Control Manager");
            ControlManager manager = manager_object.AddComponent<ControlManager>();
            GameObject selected = Selection.activeGameObject;
            if (selected != null)
            {
                Undo.SetTransformParent(manager_object.transform, selected.transform.parent, "Add Control Manager");
                manager.Subject = selected;
            }
            return manager;
        }
#endif
    }
}
