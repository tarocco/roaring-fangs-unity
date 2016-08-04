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

using System.Collections.Generic;
using UnityEngine;

namespace RoaringFangs.Motion
{
    [ExecuteInEditMode]
    public class Drifter : MonoBehaviour
    {
        public Transform TargetA;
        public Transform TargetB;
        public float Interval = 1f;
        private float _TimeTargetTick;
        private float _TimeDrawLineSegment;

        public struct LineSegment
        {
            public Vector3 p0;
            public Vector3 p1;
        }

        private List<LineSegment> LineSegments = new List<LineSegment>();

        private IEnumerator<float> TickTarget_F()
        {
            for (;;)
            {
                GetComponent<BouncyMove>().TargetPositionWorld = TargetA.transform.position;
                yield return Interval;
                GetComponent<BouncyMove>().TargetPositionWorld = TargetB.transform.position;
                yield return Interval;
            }
        }

        private IEnumerator<WaitForSeconds> TickTarget()
        { yield return new WaitForSeconds(TickTarget_F().Current); }

        private IEnumerator<float> TickSegment_F()
        {
            for (;;)
            {
                int last_idx = LineSegments.Count - 1;
                Vector3 p = last_idx > 0 ? LineSegments[last_idx].p1 : transform.position;
                LineSegments.Add(new LineSegment() { p0 = p, p1 = p });
                yield return 0.002f;
            }
        }

        private IEnumerator<float> _TickTarget_F;
        private IEnumerator<float> _TickSegment_F;

        private void Start()
        {
            //StartCoroutine("Tick");
            _TickTarget_F = TickTarget_F();
            _TickSegment_F = TickSegment_F();
        }

        private void Update()
        {
            if (Time.time > _TimeTargetTick)
            {
                _TickTarget_F.MoveNext();
                _TimeTargetTick = Time.time + _TickTarget_F.Current;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = GetComponent<SpriteRenderer>().color;
            if (Time.time > _TimeDrawLineSegment)
            {
                _TickSegment_F.MoveNext();
                _TimeDrawLineSegment = Time.time + _TickSegment_F.Current;
            }
            if (LineSegments.Count > 0)
            {
                int last_idx = LineSegments.Count - 1;
                LineSegment s = LineSegments[last_idx];
                s.p1 = transform.position;
                LineSegments[last_idx] = s;
            }
            foreach (LineSegment segment in LineSegments)
            {
                Gizmos.DrawLine(segment.p0, segment.p1);
            }
        }
    }
}