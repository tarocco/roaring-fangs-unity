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
using System.Linq;

namespace RoaringFangs.ASM
{
    /// <summary>
    /// TODO: agnosticize *State and *StateMachine behaviors
    /// </summary>
    public class SceneStateMachine : SceneStateBase
    {
        [SerializeField]
        [Tooltip("If enabled, OnStateMachineEnter will be raised on parent " +
                 "state machines for the layer default state when the " +
                 "animator starts.")]
        private bool _EnterAtStart = true;

        public bool EnterAtStart
        {
            get { return _EnterAtStart; }
            set { _EnterAtStart = value; }
        }
        private bool _IsActive;
        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo state_info,
            int layer_index)
        {
            // Do this here because normally OnStateMachineEnter is not
            // raised if we started inside this state machine.
            if (EnterAtStart && !_IsActive)
                OnStateMachineEnter(animator, state_info.fullPathHash);
            _IsActive = true;
        }

        public override void OnStateExit(
            Animator animator,
            AnimatorStateInfo state_info,
            int layer_index)
        {
            // Don't set _IsActive to false here because
            // OnStateExit is called after OnStateEnter.

            // Changing sub-states would incorrectly leave
            // _IsActive set to false.
        }

        public override void OnStateMachineEnter(Animator animator, int state_machine_path_hash)
        {
            base.OnStateMachineEnter(animator, state_machine_path_hash);
            _IsActive = true;
        }

        public override void OnStateMachineExit(Animator animator, int state_machine_path_hash)
        {
            base.OnStateMachineExit(animator, state_machine_path_hash);
            _IsActive = false;
        }
    }
}
