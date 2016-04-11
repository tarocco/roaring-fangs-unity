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
using System.Collections;

namespace RoaringFangs.Motion
{
    public class BouncyMove : TimeScaleIndependentUpdate, IHasTarget, IHasTargetPosition<Vector3>
    {
        public Transform _Target;
        public Transform Target { get { return _Target; } set { _Target = value; } }
        public int Stagger = 1;
        private int StaggerIndex = 0;

        public static readonly float TimeSubStepEpsilon = 0.001f;
        [Range(0.001f, 0.100f)] // CS0182
        public float TimeSubStep = 0.01f;

        [System.Serializable]
        public struct SecondOrderCoefficients
        {
            public float Spring, Drag;
        }
        public SecondOrderCoefficients Coefficients = new SecondOrderCoefficients()
        {
            Spring = 1f,
            Drag = 1f
        };

        public Vector3 _TargetPosition;
        public Vector3 TargetPosition
        {
            get
            {
                if (Target == null)
                    return _TargetPosition;
                if (transform.parent == null)
                    return Target.position;
                return transform.parent.InverseTransformPoint(Target.position);
            }
            set
            {
                if (Target == null)
                    _TargetPosition = value;
            }
        }

        protected Vector3 _Position;
        protected Vector3 _dPosition;

        void Start()
        {
            _TargetPosition = transform.localPosition;
        }
        float _dt;
        protected override void Update()
        {
            base.Update();

            // TimeSubStep must be reasonably greater than 0
            // or else it will create an infinite loop or lag
            TimeSubStep = Mathf.Max(TimeSubStep, TimeSubStepEpsilon);

            _dt += deltaTime; // TimeScaleIndependentUpdate
            if (StaggerIndex % Stagger == 0)
            {
                // TODO: Model this as a differential equation and use RK4 solver
                for (; _dt > 0f; _dt -= TimeSubStep)
                {
                    float d = Mathf.Min(TimeSubStep, _dt);
                    _TargetPosition = TargetPosition;
                    Vector3 drag = Coefficients.Drag * (transform.localPosition - _Position);
                    Vector3 force = Coefficients.Spring * (_TargetPosition - transform.localPosition);
                    Vector3 force_dt = force * d;

                    _Position = transform.localPosition;
                    transform.localPosition += (0.5f * force_dt + _dPosition - drag) * d;
                    _dPosition += force_dt - drag;
                }
                _dt = 0f;
            }
            StaggerIndex = (StaggerIndex + 1) % Stagger;
        }
    }
}