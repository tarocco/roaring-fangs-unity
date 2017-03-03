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

#if LIGHTSTRIKE_ADVANCED_INSPECTOR

using AdvancedInspector;

#endif

using UnityEngine;

namespace RoaringFangs.ASM
{
#if LIGHTSTRIKE_ADVANCED_INSPECTOR
    public abstract class StateController : AIStateMachineBehaviour, IStateController
#else
    public abstract class StateController : StateMachineBehaviour, IStateController
#endif
    {
        public abstract string Tag { get; set; }

        private ControlledStateManager _CachedControlledStateManager;

        protected ControlledStateManager CachedControlledStateManager
        {
            get { return _CachedControlledStateManager; }
            set { _CachedControlledStateManager = value; }
        }

        public static bool GetSceneController(Animator animator, out ControlledStateManager aggregator)
        {
            aggregator = animator.GetComponent<ControlledStateManager>();
            if (aggregator != null)
                return true;
            Debug.LogWarning("ControlledStateManager not found on Animator GameObject");
            return false;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo state_info, int layer_index)
        {
            var args = new StateControllerEventArgs(animator, state_info, layer_index);
            CachedControlledStateManager.OnStateControllerEntry(this, args);
        }

        public virtual void OnManagedStateEnter(ControlledStateManager manager, ManagedStateEventArgs args)
        {
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo state_info, int layer_index)
        {
            var args = new StateControllerEventArgs(animator, state_info, layer_index);
            CachedControlledStateManager.OnStateControllerUpdate(this, args);
        }

        public virtual void OnManagedStateUpdate(ControlledStateManager manager, ManagedStateEventArgs args)
        {
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo state_info, int layer_index)
        {
            var args = new StateControllerEventArgs(animator, state_info, layer_index);
            CachedControlledStateManager.OnStateControllerExit(this, args);
        }

        public virtual void OnManagedStateExit(ControlledStateManager manager, ManagedStateEventArgs args)
        {
        }

        public virtual void Initialize(ControlledStateManager manager)
        {
            CachedControlledStateManager = manager;
        }

        public virtual void OnManagedStateVerifyEnter(ControlledStateManager manager, ManagedStateEventArgs args)
        {
        }

        public virtual void OnManagedStateVerifyUpdate(ControlledStateManager manager, ManagedStateEventArgs args)
        {
        }

        public virtual void OnManagedStateVerifyExit(ControlledStateManager manager, ManagedStateEventArgs args)
        {
        }
    }
}