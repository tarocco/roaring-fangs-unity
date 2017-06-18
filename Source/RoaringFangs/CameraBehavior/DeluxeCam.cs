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

using RoaringFangs.Motion;
using UnityEngine;

namespace RoaringFangs.CameraBehavior
{
    public class DeluxeCam : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        private GameObject _FollowTarget;

        [SerializeField]
        private MonoBehaviour _MovementVectorTarget;

        //[SerializeField]
        //private Rigidbody _RigidbodyTarget;

        public GameObject FollowTarget
        {
            get { return _FollowTarget; }
            set
            {
                _FollowTarget = value;
                if (value)
                {
                    MovementVectorTarget = value.GetComponent<IHasMovementVector>();
                    //RigidbodyTarget = value.GetComponent<Rigidbody>();
                }
                else
                {
                    MovementVectorTarget = null;
                    //RigidbodyTarget = null;
                }
            }
        }

        protected IHasMovementVector MovementVectorTarget
        {
            get { return _MovementVectorTarget as IHasMovementVector; }
            private set { _MovementVectorTarget = value as MonoBehaviour; }
        }

        //protected Rigidbody RigidbodyTarget
        //{
        //    get { return _RigidbodyTarget; }
        //    private set { _RigidbodyTarget = value; }
        //}

        [SerializeField] private Rigidbody _Rigidbody;

        public Rigidbody Rigidbody
        {
            get { return _Rigidbody; }
            set { _Rigidbody = value; }
        }

        [SerializeField] private Vector2 _AlignmentScale = Vector2.one;

        public Vector2 AlignmentScale
        {
            get { return _AlignmentScale; }
            set { _AlignmentScale = value; }
        }

        [SerializeField] private AnimationCurve _HorizontalAlignmentCurve =
            AnimationCurve.Linear(0.0f, 0.5f, 1.0f, 0.5f);

        public AnimationCurve HorizontalAlignmentCurve
        {
            get { return _HorizontalAlignmentCurve; }
            set { _HorizontalAlignmentCurve = value; }
        }

        [SerializeField] private AnimationCurve _VerticalAlignmentCurve =
            AnimationCurve.Linear(0.0f, 0.5f, 1.0f, 0.5f);

        public AnimationCurve VerticalAlignmentCurve
        {
            get { return _VerticalAlignmentCurve; }
            set { _VerticalAlignmentCurve = value; }
        }

        [SerializeField] private Vector2 _Lookahead;

        public Vector2 Lookahead
        {
            get { return _Lookahead; }
            set
            {
                _Lookahead = new Vector2(
                    Mathf.Max(0f, value.x),
                    Mathf.Max(0f, value.y));
            }
        }

        [SerializeField] private Vector2 _LookaheadMin = new Vector2(-999, -999);

        public Vector2 LookaheadMin
        {
            get { return _LookaheadMin; }
            set
            {
                _LookaheadMin = new Vector2(
                    Mathf.Min(value.x, _LookaheadMax.x),
                    Mathf.Min(value.y, _LookaheadMax.y));
            }
        }

        [SerializeField] private Vector2 _LookaheadMax = new Vector2(999, 999);

        public Vector2 LookaheadMax
        {
            get { return _LookaheadMax; }
            set
            {
                _LookaheadMax = new Vector2(
                    Mathf.Max(value.x, _LookaheadMin.x),
                    Mathf.Max(value.y, _LookaheadMin.y));
            }
        }

        private void Start()
        {
        }

        private Vector3 CalculateNextPosition(
            Vector3 target_position,
            Vector3 alignment_position)
        {
            // TODO: axis-independent calculations (relative horizontal/vertical axes)
            var difference = target_position - alignment_position;

            var x_interpolant =
                AlignmentScale.x*
                HorizontalAlignmentCurve.Evaluate(Mathf.Abs(difference.x));
            var y_interpolant =
                AlignmentScale.y*
                VerticalAlignmentCurve.Evaluate(Mathf.Abs(difference.y));
            var next_point = new Vector3(
                Mathf.Lerp(alignment_position.x, target_position.x, x_interpolant),
                Mathf.Lerp(alignment_position.y, target_position.y, y_interpolant),
                alignment_position.z);
            return next_point;
        }

        private void FixedUpdate()
        {
            if (!FollowTarget)
                return;

            var target_position = FollowTarget.transform.position;
            Vector3 lookahead_vector;

            if (MovementVectorTarget != null)
            {
                lookahead_vector = MovementVectorTarget.MovementVector;
                lookahead_vector = new Vector3(
                    Mathf.Clamp(Lookahead.x*lookahead_vector.x, LookaheadMin.x, LookaheadMax.x),
                    Mathf.Clamp(Lookahead.y*lookahead_vector.y, LookaheadMin.y, LookaheadMax.y),
                    lookahead_vector.z);
            }
            else
                lookahead_vector = Vector3.zero;

            var next_position = CalculateNextPosition(
                target_position + lookahead_vector,
                transform.position);

            // Direct approach
            // transform.position = next_position;

            // Velocity approach

            var delta = next_position - transform.position;
            var velocity = delta/Time.fixedDeltaTime;
            Rigidbody.velocity = velocity;
        }

        public void OnBeforeSerialize()
        {
            Rigidbody = GetComponent<Rigidbody>();
            // Round trips
            Lookahead = Lookahead;
            FollowTarget = FollowTarget;
        }

        public void OnAfterDeserialize()
        {
        }
    }
}