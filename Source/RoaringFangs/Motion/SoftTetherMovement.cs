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

namespace RoaringFangs.Motion
{
    public class SoftTetherMovement : MonoBehaviour, IPositionTethered, IUpdatesPosition
    {
        [SerializeField]
        private Transform _Target;

        public Transform Target
        {
            get { return _Target;}
            set { _Target = value; }
        }

        public Vector3 TargetOffset;

        [SerializeField]
        private Transform _Anchor;

        public Transform Anchor
        {
            get { return _Anchor; }
            set { _Anchor = value; }
        }

        public float FollowDistanceMin = 0f;
        public float FollowDistanceMax = float.PositiveInfinity;

        public float SmoothingFactor = 1.5f;

        private void Start()
        {
        }

        private float SoftLimit(float t)
        {
            return 0.5f * (Mathf.Cos(Mathf.PI * (t + 1f)) + 1f);
        }

        private float SoftLimitOut(float t)
        {
            return Mathf.Sin(Mathf.PI * (0.5f * t));
        }

        private float SoftLimitOut2(float t)
        {
            return 1f - Mathf.Pow(t - 1f, 2f);
        }

        public void UpdatePosition()
        {
            if (Target != null)
            {
                Vector3 follow_position;
                if (_Anchor != null)
                {
                    Vector3 tether_position = _Anchor.position;
                    Vector3 difference = Target.position - tether_position;
                    Vector3 difference_inverse = _Anchor.InverseTransformVector(difference);
                    float alpha = Mathf.Clamp01((difference_inverse.magnitude - FollowDistanceMin) / (SmoothingFactor * FollowDistanceMax));
                    float beta = FollowDistanceMin + SoftLimitOut2(alpha) * (FollowDistanceMax - FollowDistanceMin);
                    difference_inverse = difference_inverse.normalized * beta;
                    difference = _Anchor.TransformVector(difference_inverse);
                    follow_position = tether_position + difference;
                }
                else
                    follow_position = Target.position;
                transform.position = follow_position + TargetOffset;
            }
        }

        private void Update()
        {
            UpdatePosition();
        }
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (_Anchor != null)
            {
                Gizmos.matrix = _Anchor.localToWorldMatrix;
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(Vector3.zero, FollowDistanceMin);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(Vector3.zero, FollowDistanceMax);
            }
        }
#endif
    }
}