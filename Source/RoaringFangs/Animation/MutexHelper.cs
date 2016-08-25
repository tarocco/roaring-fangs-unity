﻿/*
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
using RoaringFangs.Attributes;
using RoaringFangs.Utility;
using System.Linq;

namespace RoaringFangs.Animation
{
    [ExecuteInEditMode]
    public class MutexHelper : MonoBehaviour
    {
        [SerializeField, AutoRange(0f, "NumberOfControls")]
        private float _ControlSelect;

        public float ControlSelect
        {
            get { return _ControlSelect; }
            set
            {
                _CachedControls = Controls.ToArray();
                if (_CachedControls.Length > 0)
                {
                    Selected = GetControlByInterpolant(_CachedControls, value);
                    _ControlSelect = value;
                }
            }
        }

        private ITargetGroup GetControlByInterpolant(ITargetGroup[] target_groups, float interpolant)
        {
            int index = Mathf.Clamp(Mathf.FloorToInt(interpolant), 0, target_groups.Length - 1);
            return target_groups[index];
        }

        private int NumberOfControls
        {
            get
            {
                return (int)Controls.LongCount();
            }
        }

        private ITargetGroup[] _CachedControls;

        public IEnumerable<ITargetGroup> Controls
        {
            get
            {
                foreach (Transform t in transform)
                {
                    ITargetGroup ac = t.GetComponent<ITargetGroup>();
                    if (ac != null)
                        yield return ac;
                }
            }
        }

        private ITargetGroup _Selected;

        public ITargetGroup Selected
        {
            get
            {
                if (_Selected == null)
                    ControlSelect = ControlSelect; // Lazy initialize
                return _Selected;
            }
            set
            {
                _CachedControls = Controls.ToArray();
                if (!_CachedControls.Contains(value))
                    throw new ArgumentException("Cannot select non-child target group");
                if (!(value is MonoBehaviour))
                    throw new ArgumentException("Selected target group must inherit MonoBehaviour");
                _Selected = value;
                _ControlSelect = Array.IndexOf(_CachedControls, _Selected);
                SetVisibleGroup(_CachedControls, value);
            }
        }

        private void SetVisibleGroup(ITargetGroup[] target_groups, ITargetGroup selected)
        {
            foreach (ITargetGroup control in target_groups)
                control.Active = false;
            selected.Active = true;
        }

        void Start()
        {
            ControlSelect = ControlSelect; // Lazy initialize
        }

        void LateUpdate()
        {
            if (_CachedControls != null && _CachedControls.Length > 0)
            {
                _Selected = GetControlByInterpolant(_CachedControls, _ControlSelect);
                SetVisibleGroup(_CachedControls, _Selected);
            }
        }

#if UNITY_EDITOR

        [MenuItem("Roaring Fangs/Animation/Mutex Helper", false, 0)]
        [MenuItem("GameObject/Roaring Fangs/Animation/Mutex Helper", false, 0)]
        [MenuItem("CONTEXT/ControlManager/Mutex Helper", false, 25)]
        public static MutexHelper Create()
        {
            GameObject selected = Selection.activeGameObject;
            ControlManager manager;
            if (selected != null)
                manager = selected.GetComponentInParent<ControlManager>();
            else
                manager = null;
            if (manager == null)
                throw new Exception("You must select a control for this target group.");

            GameObject mutex_helper_object = new GameObject("New Mutex Helper");
            Undo.RegisterCreatedObjectUndo(mutex_helper_object, "New Mutex Helper");
            MutexHelper mutex_helper = mutex_helper_object.AddComponent<MutexHelper>();

            Undo.SetTransformParent(mutex_helper_object.transform, selected.transform, "New Mutex Helper");
            Selection.activeGameObject = mutex_helper_object;
            return mutex_helper;
        }

#endif
    }
}