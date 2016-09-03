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

using RoaringFangs.Attributes;
using System.Linq;
using UnityEngine;

namespace RoaringFangs.Animation
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MutexHelper))]
    public class LinearMutexSelector : MonoBehaviour
    {
        [SerializeField]
        private MutexHelper _MutexHelper;

        public MutexHelper MutexHelper
        {
            get
            {
                if (_MutexHelper == null || _MutexHelper.gameObject != gameObject)
                    _MutexHelper = GetComponent<MutexHelper>();
                Debug.Assert(_MutexHelper != null, "MutexHelper must not be null!");
                return _MutexHelper;
            }
            set { _MutexHelper = value; }
        }

        private ITargetGroup _CachedSelected;

        private ITargetGroup CachedSelected
        {
            get
            {
                if (_CachedSelected == null)
                    _CachedSelected = Selected;
                return _CachedSelected;
            }
            set
            {
                _CachedSelected = value;
            }
        }

        private ITargetGroup Selected
        {
            get { return MutexHelper.Selected; }
            set
            {
                CachedSelected = value; // Handled by lazy evaluation
                MutexHelper.Selected = value;
            }
        }

        private ITargetGroup GetCachedControlByIndex(int index)
        {
            return MutexHelper.GetCachedControlByIndex(index);
        }

        private int NumberOfCachedControls
        {
            get
            {
                return MutexHelper.NumberOfCachedControls;
            }
        }

        private int GetSelectorIndex(float selector)
        {
            var floored_interpolant = Mathf.FloorToInt(selector);
            var upper_bound = NumberOfCachedControls - 1;
            var index = Mathf.Clamp(floored_interpolant, 0, upper_bound);
            return index;
        }

        [SerializeField, AutoRange(0f, "NumberOfCachedControls")]
        private float _Selector;

        public float Selector
        {
            get { return _Selector; }
            set
            {
                //_CachedControls = Controls.ToArray();
                if (NumberOfCachedControls > 0)
                {
                    int index = GetSelectorIndex(value);
                    var selected = GetCachedControlByIndex(index);
                    Selected = selected;
                    _Selector = value;
                }
            }
        }

        private int GetCachedControlIndex(ITargetGroup target_group)
        {
            return Enumerable.Range(0, NumberOfCachedControls)
                .Select(i => GetCachedControlByIndex(i))
                .TakeWhile(c => !target_group.Equals(c))
                .Count();
        }

        private void Start()
        {
        }

        private void LateUpdate()
        {
            if (NumberOfCachedControls > 0)
            {
                int selector_index = GetSelectorIndex(Selector);
                var selected_by_selector = GetCachedControlByIndex(selector_index);
                if (CachedSelected != Selected) // If the selected control was changed externally
                {
                    // Update the selector-selected control to the mutex helper's selected control
                    _Selector = GetCachedControlIndex(Selected);
                    CachedSelected = Selected;
                }
                else if (selected_by_selector != Selected) // If the change was from here
                {
                    // Update the mutex helper's selected control to the selector-selected control
                    Selected = selected_by_selector;
                }
            }
        }
    }
}