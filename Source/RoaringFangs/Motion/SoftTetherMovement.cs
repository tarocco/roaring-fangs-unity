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
    public class SoftTetherMovement : MonoBehaviour
    {
        public Transform Target;
        public Transform Tether;

        public Vector3 Offset;

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

        private void Update()
        {
            if (Target != null)
            {
                Vector3 follow_position;
                if (Tether != null)
                {
                    Vector3 difference = Target.position - Tether.position;
                    float alpha = Mathf.Clamp01((difference.magnitude - FollowDistanceMin) / (SmoothingFactor * FollowDistanceMax));
                    float beta = FollowDistanceMin + SoftLimitOut2(alpha) * (FollowDistanceMax - FollowDistanceMin);
                    difference = difference.normalized * beta;
                    follow_position = Tether.position + difference;
                }
                else
                    follow_position = Target.position;
                transform.position = follow_position + Offset;
            }
        }

        /*
        void OnDrawGizmos()
        {
            for(int i = 0; i < 32; i ++)
            {
                float x = (float)i / 31f;
                Gizmos.DrawSphere(new Vector3(x, SoftLimitOut(x), 0), 0.1f);
            }
        }
        */
    }
}