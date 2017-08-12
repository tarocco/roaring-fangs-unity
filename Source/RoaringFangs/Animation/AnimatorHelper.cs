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
using RoaringFangs.Attributes;
using UnityEngine;
using System.Linq;

namespace RoaringFangs.Animation
{
    public class AnimatorHelper : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField, AutoProperty]
        private Animator _Animator;

        public Animator Animator
        {
            get
            {
                if (_Animator == null)
                    _Animator = GetComponent<Animator>();
                return _Animator;
            }
            protected set { _Animator = value; }
        }

        [SerializeField]
        private string _FloatParameterName;

        public string FloatParameterName
        {
            get { return _FloatParameterName; }
            set
            {
                _FloatParameterName = value;
                FloatParameter = null;
            }
        }

        private AnimatorControllerParameter _FloatParameter;

        public AnimatorControllerParameter FloatParameter
        {
            get
            {
                if (Animator != null && _FloatParameter == null)
                {
                    if (string.IsNullOrEmpty(FloatParameterName))
                        _FloatParameter = null;
                    else
                        _FloatParameter = Animator.parameters
                            .FirstOrDefault(p => p.name == FloatParameterName);
                }
                return _FloatParameter;
            }
            private set
            {
                _FloatParameter = value;
            }
        }

        [SerializeField]
        private float _FloatValue;

        public float FloatValue
        {
            get { return _FloatValue; }
            set
            {
                _FloatValue = value;
                if (Animator != null && FloatParameter != null)
                {
                    var fx = Evaluate(value);
                    Animator.SetFloat(FloatParameterName, fx);
                }
            }
        }

        [SerializeField]
        private bool _UseFloatValueCurve;

        public bool UseFloatValueCurve
        {
            get { return _UseFloatValueCurve; }
            set { _UseFloatValueCurve = value; }
        }

        [SerializeField]
        private AnimationCurve _FloatValueCurve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public AnimationCurve FloatValueCurve
        {
            get { return _FloatValueCurve; }
            private set { _FloatValueCurve = value; }
        }

        private float Evaluate(float x)
        {
            if (UseFloatValueCurve)
                return FloatValueCurve.Evaluate(x);
            return x;
        }

        public void SetTrigger(string name)
        {
            Animator.SetTrigger(name);
        }

        public void ResetTrigger(string name)
        {
            Animator.ResetTrigger(name);
        }

        public void OnBeforeSerialize()
        {
            if (Animator == null)
                Animator = GetComponent<Animator>();
            FloatParameterName = FloatParameterName;
        }

        public void OnAfterDeserialize()
        {
        }

        private void Start()
        {
        }

        private void Update()
        {
            FloatValue = FloatValue;
        }
    }
}