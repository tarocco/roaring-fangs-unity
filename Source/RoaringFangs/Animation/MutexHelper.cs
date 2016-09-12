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
using RoaringFangs.Attributes;
using RoaringFangs.Utility;
using System.Linq;

namespace RoaringFangs.Animation
{
    [ExecuteInEditMode]
    public class MutexHelper : MonoBehaviour
    {
        public IEnumerable<ITargetGroup> Controls
        {
            get
            {
                foreach (Transform t in transform)
                {
                    ITargetGroup ac = t.GetComponent<ITargetGroup>();
                    if (ac != null)
                    {
                        yield return ac;
                    }
                }
            }
        }

        private ITargetGroup[] _CachedControls;

        private ITargetGroup[] CachedControls
        {
            get
            {
                if(_CachedControls == null)
                    _CachedControls = Controls.ToArray();
                return _CachedControls;
            }
        }

        public ITargetGroup GetCachedControlByIndex(int index)
        {
            return CachedControls[index];
        }

        public int NumberOfCachedControls
        {
            get
            {
                return (int)CachedControls.LongCount();
            }
        }

        [SerializeField, HideInInspector]
        private MonoBehaviour _SelectedBehavior;

        public ITargetGroup Selected
        {
            get
            {
                if (_SelectedBehavior == null)
                    SetSelected(CachedControls.First());
                return (ITargetGroup)_SelectedBehavior;
            }
            set
            {
                _CachedControls = null;
                SetSelected(value);
            }
        }

        private void SetSelected(ITargetGroup value)
        {
            if (!CachedControls.Contains(value))
                throw new ArgumentException("Cannot select non-child target group");
            if (!(value is MonoBehaviour))
                throw new ArgumentException("Selected target group must inherit MonoBehaviour");
            _SelectedBehavior = (MonoBehaviour)value;
            SetVisibleGroup(CachedControls, value);
        }

        private void SetVisibleGroup(ITargetGroup[] target_groups, ITargetGroup selected)
        {
            foreach (ITargetGroup control in target_groups)
                control.SetActive(false);
            selected.SetActive(true);
        }

        void Start()
        {
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