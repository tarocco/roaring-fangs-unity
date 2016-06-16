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
                if (_Subject == null)
                {
                    if (!String.IsNullOrEmpty(SubjectPath))
                    {
                        string parent_path;
                        if (transform.parent != null)
                            parent_path = TransformUtils.GetTransformPath(null, transform.parent);
                        else
                            parent_path = "";
                        string abs_path;
                        if (_SubjectPath.StartsWith("../"))
                            abs_path = parent_path + _SubjectPath.Substring(2);
                        else if (_SubjectPath.StartsWith("/"))
                            abs_path = _SubjectPath;
                        else
                            abs_path = parent_path + _SubjectPath;
                        _Subject = GameObject.Find(abs_path);
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
                    CachedSubjectDescendantsAndPaths = CollectSubjectDescendants();
                    NotifyControlGroupsOfSubjectDescendants();
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
                    if (String.IsNullOrEmpty(value))
                        Subject = null;
                    _SubjectPath = value;
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
        [SerializeField, HideInInspector]
        private TransformUtils.TransformDP[] _CachedSubjectDescendantsAndPaths;
        public IEnumerable<TransformUtils.ITransformDP> CachedSubjectDescendantsAndPaths
        {
            get
            {
                // Lazy initialization
                if (_CachedSubjectDescendantsAndPaths == null)
                {
                    CachedSubjectDescendantsAndPaths = CollectSubjectDescendants();
                }
                // Double null check for quality assurance
                if (_CachedSubjectDescendantsAndPaths != null)
                {
                    foreach (var tdp in _CachedSubjectDescendantsAndPaths)
                        yield return tdp;
                }
            }
            private set
            {
                if (value != null)
                {
                    // Create a concrete struct array from the abstract (interface) enumerable
                    _CachedSubjectDescendantsAndPaths = value
                        .Select(t => new TransformUtils.TransformDP(t.Transform, t.Depth, t.Path))
                        .ToArray();
                    NotifyControlGroupsOfSubjectDescendants(value);
                }
                else if(_CachedSubjectDescendantsAndPaths != null)
                {
                    _CachedSubjectDescendantsAndPaths = null;
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
            var groups = TransformUtils.GetComponentsInDescendants<TargetGroupBehavior>(transform, true);
            foreach (var group in groups)
                group.OnSubjectChanged(cached_subject_descendants_and_paths);
        }

        protected IEnumerable<TransformUtils.ITransformDP> CollectSubjectDescendants()
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
            public readonly bool Active;
            public TargetInfo(int depth, bool active)
            {
                Depth = depth;
                Active = active;
            }
        }

        private struct TargetRule
        {
            public readonly Transform Transform;
            public readonly TargetInfo Info;
            public TargetRule(Transform transform, int depth, bool active)
            {
                Transform = transform;
                Info = new TargetInfo(depth, active);
            }
        }
        private Dictionary<Transform, TargetInfo> TargetDataPrevious = new Dictionary<Transform, TargetInfo>();
        #endregion

        private bool _FirstEnable = true;
        void OnEnable()
        {
            if (_FirstEnable)
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    CollectSubjectDescendants();
                    RoaringFangs.Editor.EditorHelper.HierarchyObjectPathChanged += HandleHierarchyObjectPathChanged;
                }
#endif
                _FirstEnable = false;
            }
        }

        private void HandleHierarchyObjectPathChanged(object sender, RoaringFangs.Editor.EditorHelper.HierarchyObjectPathChangedEventArgs args)
        {
            // If the change had anything to do with the subject
            if (!String.IsNullOrEmpty(SubjectPathAbs) &&
                ((!String.IsNullOrEmpty(args.NewPath) && args.NewPath.StartsWith(SubjectPathAbs) ||
                (!String.IsNullOrEmpty(args.OldPath) && args.OldPath.StartsWith(SubjectPathAbs)))))
            {
                // Lazily invalidate the cached subject descentants and paths
                CachedSubjectDescendantsAndPaths = null;
            }
        }

        void Update()
        {
            var target_data_previous = new Dictionary<Transform, TargetInfo>();
            //var groups = TransformUtils.GetComponentsInDescendants<TargetGroupBehavior>(transform, true);
            // all those yields and enumerables are cool and everything, but i'd like to be safe at least for debugging
            var groups = transform.GetComponentsInChildren<TargetGroupBehavior>(true);
            IEnumerable<TargetRule> targets = groups
                .Where(g => g.Targets != null)
                .SelectMany(g => g.Targets
                    .Select(t => new TargetRule(t.Transform, t.Depth, g.gameObject.activeSelf)));
            var targets_array = targets.ToArray();

            // bug yep, it always returns 0 until scene restart, getting closer to the actual issue
            Debug.Log(targets_array.Length);

            foreach (var target in targets_array)
            {
                if (target.Transform != null)
                {
                    TargetInfo target_info_previous;
                    bool have_previous = TargetDataPrevious.TryGetValue(target.Transform, out target_info_previous);
                    bool active_update = !have_previous ||
                        target.Info.Active != target_info_previous.Active ||
                        target.Info.Depth != target_info_previous.Depth;
                    if (active_update)
                    {
                        // Update the target game object
                        target.Transform.gameObject.SetActive(target.Info.Active);
                    }
                    // Add to previous target data dictionary
                    target_data_previous[target.Transform] = target.Info;
                }
            }
            // Update previous value dictionary
            TargetDataPrevious = target_data_previous;
        }
#if UNITY_EDITOR
        [MenuItem("Sprites And Bones/Animation/Control Manager", false, 0)]
        [MenuItem("GameObject/Sprites And Bones/Control Manager", false, 0)]
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
