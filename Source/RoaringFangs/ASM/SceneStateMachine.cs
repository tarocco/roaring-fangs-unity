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

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace RoaringFangs.ASM
{
    /// <summary>
    /// TODO: agnosticize *State and *StateMachine behaviors
    /// </summary>
    public class SceneStateMachine : SceneState
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

#if ODIN_INSPECTOR
#pragma warning disable 414 // The private field '_LayerIndices' is assigned but its value is never used
        private static readonly int[] _LayerIndices =
            { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
#pragma warning restore 414 // The private field '_LayerIndices' is assigned but its value is never used
#endif

        [SerializeField]
#if ODIN_INSPECTOR
        [ValueDropdown("_LayerIndices")]
#endif
        private int _LayerIndex = 0;

        public int LayerIndex
        {
            get { return _LayerIndex; }
            set { _LayerIndex = value; }
        }

        private static bool IsEnabledByAnimator(Animator animator, int layer_index)
        {
            return
                layer_index == 0 ||
                (layer_index >= 0 &&
                layer_index < animator.layerCount &&
                !Mathf.Approximately(animator.GetLayerWeight(layer_index), 0f));
        }

        private bool _IsActive;

        private int _PreviousStateEnterLayerIndex = -1;

        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo state_info,
            int layer_index)
        {
            _PreviousStateEnterLayerIndex = layer_index;
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
            if (_PreviousStateEnterLayerIndex != LayerIndex)
                return;
            if (!IsEnabledByAnimator(animator, LayerIndex))
                return;
            base.OnStateMachineEnter(animator, state_machine_path_hash);
            _IsActive = true;
        }

        public override void OnStateMachineExit(Animator animator, int state_machine_path_hash)
        {
            if (_PreviousStateEnterLayerIndex != LayerIndex)
                return;
            if (!IsEnabledByAnimator(animator, LayerIndex))
                return;
            base.OnStateMachineExit(animator, state_machine_path_hash);
            _IsActive = false;
        }
    }
}