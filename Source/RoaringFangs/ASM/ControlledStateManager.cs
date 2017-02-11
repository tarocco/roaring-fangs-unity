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
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Director;

namespace RoaringFangs.ASM
{
    public class ControlledStateManager : MonoBehaviour, ISerializationCallbackReceiver
    {
        private IAnimatorControllerPlayable _Animator;

        public IAnimatorControllerPlayable Animator
        {
            get
            {
                if (_Animator == null)
                    _Animator = GetComponent<IAnimatorControllerPlayable>();
                return _Animator;
            }
            set { _Animator = value; }
        }

        [SerializeField]
        private Transform _ConfigurationObjectCache;

        public Transform ConfigurationObjectCache
        {
            get { return _ConfigurationObjectCache; }
            private set { _ConfigurationObjectCache = value; }
        }

        [SerializeField, HideInInspector]
        private List<MonoBehaviour> _StateHandlers;

        public ILookup<int, IStateHandler> StateHandlersNameHashLookup { get; private set; }
        public ILookup<int, IStateHandler> StateHandlersTagHashLookup { get; private set; }
        public ILookup<string, IStateHandler> StateHandlersNameLookup { get; private set; }
        public ILookup<string, IStateHandler> StateHandlersTagLookup { get; private set; }

        public IEnumerable<IStateHandler> StateHandlers
        {
            get { return _StateHandlers.Cast<IStateHandler>(); }
            set
            {
                _StateHandlers = value.Cast<MonoBehaviour>().ToList();
                StateHandlersNameHashLookup = value.ToLookup(h => h.NameHash);
                StateHandlersTagHashLookup = value.ToLookup(h => h.TagHash);
                StateHandlersNameLookup = value.ToLookup(h => h.Name);
                StateHandlersTagLookup = value.ToLookup(h => h.Tag);
            }
        }

        public void OnBeforeSerialize()
        {
            StateHandlers = GetComponents<IStateHandler>();
        }

        public void OnAfterDeserialize()
        {
            StateHandlers = StateHandlers;
        }

        #region Events

        [SerializeField]
        private StateEvent _AnyStateEntry;

        public StateEvent Entry
        {
            get { return _AnyStateEntry; }
            private set { _AnyStateEntry = value; }
        }

        [SerializeField]
        private StateEvent _AnyStateUpdate;

        public StateEvent Update
        {
            get { return _AnyStateUpdate; }
            private set { _AnyStateUpdate = value; }
        }

        [SerializeField]
        private StateEvent _AnyStateExit;

        public StateEvent Exit
        {
            get { return _AnyStateExit; }
            private set { _AnyStateExit = value; }
        }

        #endregion Events

        public void OnStateBehaviorEntry(StateEventArgs args)
        {
            var info = args.AnimatorStateInfo;
            var state_handlers = StateHandlersNameHashLookup[info.shortNameHash];
            foreach (var state_handler in state_handlers)
                state_handler.OnStateEnter(args.Animator, args.AnimatorStateInfo, args.LayerIndex);
            Entry.Invoke(this, args);
        }

        public void OnStateBehaviorUpdate(StateEventArgs args)
        {
            var info = args.AnimatorStateInfo;
            var state_handlers = StateHandlersNameHashLookup[info.shortNameHash];
            foreach (var state_handler in state_handlers)
                state_handler.OnStateUpdate(args.Animator, args.AnimatorStateInfo, args.LayerIndex);
            Update.Invoke(this, args);
        }

        public void OnStateBehaviorExit(StateEventArgs args)
        {
            var info = args.AnimatorStateInfo;
            var state_handlers = StateHandlersNameHashLookup[info.shortNameHash];
            foreach (var state_handler in state_handlers)
                state_handler.OnStateExit(args.Animator, args.AnimatorStateInfo, args.LayerIndex);
            Exit.Invoke(this, args);
        }

        public void SetAnimatorTrigger(string name)
        {
            Animator.SetTrigger(name);
        }

        public void SetAnimatorTrigger(int id)
        {
            Animator.SetTrigger(id);
        }

        public void ResetAnimatorTrigger(string name)
        {
            Animator.ResetTrigger(name);
        }

        public void ResetAnimatorTrigger(int id)
        {
            Animator.ResetTrigger(id);
        }

        public void Start()
        {
            var animator = GetComponent<Animator>();
            var state_controllers = animator.GetBehaviours<StateMachineBehaviour>()
                .OfType<IStateController>();
            foreach(var state_controller in state_controllers)
                state_controller.Initialize(this);
        }
    }
}