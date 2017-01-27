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

namespace RoaringFangs.ASM
{
    [DisallowMultipleComponent]
#if LIGHTSTRIKE_ADVANCED_INSPECTOR
    public class EventfulStateController : StateController
#else
    public class EventfulStateMachineBehavior : AggregatedStateMachineBehavior
#endif
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo state_info, int layer_index)
        {
            var args = new StateEventArgs(animator, state_info, layer_index);
            var controller = CachedControlledStateManager;
            if (controller ?? GetSceneController(animator, out controller))
            {
                CachedControlledStateManager = controller;
                controller.OnStateBehaviorEntry(args);
            }
            Entry.Invoke(this, args);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo state_info, int layer_index)
        {
            var args = new StateEventArgs(animator, state_info, layer_index);
            var controller = CachedControlledStateManager;
            if (controller ?? GetSceneController(animator, out controller))
            {
                CachedControlledStateManager = controller;
                controller.OnStateBehaviorUpdate(args);
            }
            Update.Invoke(this, args);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo state_info, int layer_index)
        {
            var args = new StateEventArgs(animator, state_info, layer_index);
            var controller = CachedControlledStateManager;
            if (controller ?? GetSceneController(animator, out controller))
            {
                CachedControlledStateManager = controller;
                controller.OnStateBehaviorExit(args);
            }
            Exit.Invoke(this, args);
        }

        [SerializeField]
        private StateEvent _Entry;

        public StateEvent Entry
        {
            get { return _Entry; }
            private set { _Entry = value; }
        }

        [SerializeField]
        private StateEvent _Update;

        public StateEvent Update
        {
            get { return _Update; }
            private set { _Update = value; }
        }

        [SerializeField]
        private StateEvent _Exit;

        public StateEvent Exit
        {
            get { return _Exit; }
            private set { _Exit = value; }
        }
    }
}