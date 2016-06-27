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
    [RequireComponent(typeof(BouncyMove))]
    public class BouncyMoveAutoScale : MonoBehaviour
    {
        private BouncyMove Self;
        public Rigidbody2D Target;
        public BouncyMove.SecondOrderCoefficients Coefficients;
        public BouncyMove.SecondOrderCoefficients DeltaSpeed;

        public float SpeedSmoothRateP = 10;
        public float SpeedSmoothRateV = 4;

        public AnimationCurve SpeedCurve;

        private Vector2 TargetPosition;
        private Vector2 TargetVelocity;
        void Start()
        {
            Self = GetComponent<BouncyMove>();
        }

        // Update is called once per frame
        void Update()
        {
            TargetPosition = Vector2.Lerp(TargetPosition, Target.position, SpeedSmoothRateP * Time.smoothDeltaTime);
            TargetVelocity = Vector2.Lerp(TargetVelocity, Target.velocity, SpeedSmoothRateV * Time.smoothDeltaTime);

            float speed = TargetVelocity.magnitude;
            float curve = SpeedCurve.Evaluate(speed);
            Self.Coefficients.Drag = Coefficients.Drag + DeltaSpeed.Drag * curve;
            Self.Coefficients.Spring = Coefficients.Spring + DeltaSpeed.Spring * curve;
        }
    }
}