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
using UnityEngine;

namespace RoaringFangs.Motion
{
    public class BouncyMove : MonoBehaviour
    {
        public static readonly float TimeSubStepEpsilon = 0.001f;

        [Range(1f, 5000f)]
        public float Spring = 50f;

        [Range(1f, 100f)]
        public float Drag = 5f;

        [SerializeField]
        private Vector3 _TargetPositionLocal;

        public Vector3 TargetPositionLocal
        {
            get { return _TargetPositionLocal; }
            set { _TargetPositionLocal = value; }
        }

        public Vector3 TargetPositionWorld
        {
            get
            {
                if (!transform.parent)
                    return _TargetPositionLocal;
                else
                    return transform.parent.TransformPoint(_TargetPositionLocal);
            }
            set
            {
                if (transform.parent == null)
                    _TargetPositionLocal = value;
                else
                    _TargetPositionLocal = transform.parent.InverseTransformPoint(value);
            }
        }

        [SerializeField, AutoProperty]
        private bool _UseWorldPosition = false;

        public bool UseWorldPosition
        {
            get { return _UseWorldPosition; }
            set
            {
                Vector3 target_position = TargetPosition;
                _UseWorldPosition = value;
                Vector3 difference = TargetPosition - target_position;
                _PreviousPosition += difference;
                _PreviousPosition2 += difference;
            }
        }

        private Vector3 _PreviousPosition;
        private Vector3 _PreviousPosition2;
        private Vector3 _IntegratedVelocity;

        protected Vector3 Position
        {
            get
            {
                if (UseWorldPosition)
                    return transform.position;
                else
                    return transform.localPosition;
            }
            set
            {
                if (UseWorldPosition)
                    transform.position = value;
                else
                    transform.localPosition = value;
            }
        }

        protected Vector3 TargetPosition
        {
            get
            {
                if (UseWorldPosition)
                    return TargetPositionWorld;
                else
                    return TargetPositionLocal;
            }
            set
            {
                if (UseWorldPosition)
                    TargetPositionWorld = value;
                else
                    TargetPositionLocal = value;
            }
        }

        [Range(0.001f, 0.100f)]
        public float TimeSubStep = 0.01f;

        public int Stagger = 1;
        private int StaggerIndex = 0;

        private void Start()
        {
        }

        private float _dt;

        private void Update()
        {
            // TimeSubStep must be reasonably greater than 0
            // or else it will create an infinite loop or lag
            TimeSubStep = Mathf.Max(TimeSubStep, TimeSubStepEpsilon);

            _dt += Time.deltaTime;
            if (StaggerIndex % Stagger == 0)
            {
                Vector3 position = Position;
                for (; _dt > 0f; _dt -= TimeSubStep)
                {
                    float d = Mathf.Min(TimeSubStep, _dt);
                    Vector3 drag = Drag * (_PreviousPosition - _PreviousPosition2);
                    Vector3 force = Spring * (TargetPosition - _PreviousPosition);
                    Vector3 force_dt = force * d;

                    _PreviousPosition2 = _PreviousPosition;
                    position = _PreviousPosition + (0.5f * force_dt + _IntegratedVelocity - drag) * d;
                    _PreviousPosition = position;
                    _IntegratedVelocity += force_dt - drag;
                }
                Position = position;
                _dt = 0f;
            }
            StaggerIndex = (StaggerIndex + 1) % Stagger;
        }
    }
}