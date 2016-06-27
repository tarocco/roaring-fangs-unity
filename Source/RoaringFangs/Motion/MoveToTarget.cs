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
    public class MoveToTarget : StateMachineBehaviour
    {
        public float Speed = 1f;
        private RectTransform RectTransform;
        private float MaxDeltaTimeRecip;
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            RectTransform = animator.GetComponent<RectTransform>();
            MaxDeltaTimeRecip = 1f / Time.maximumDeltaTime;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Vector2 target = new Vector2(
                animator.GetFloat("TargetX"),
                animator.GetFloat("TargetY"));
            Vector2 a_pos = RectTransform.anchoredPosition;
            Vector2 difference = target - a_pos;
            float distance = difference.magnitude;
            if (distance > 0f)
            {
                float scale = Mathf.Min(distance, Speed * Time.smoothDeltaTime * MaxDeltaTimeRecip);
                Vector2 move = difference.normalized * scale;
                RectTransform.anchoredPosition = a_pos + move;
            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}
    }
}