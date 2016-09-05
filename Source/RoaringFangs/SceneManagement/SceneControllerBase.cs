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

using RoaringFangs.FSM;
using RoaringFangs.Utility;
using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace RoaringFangs.SceneManagement
{
    public abstract class SceneControllerBase<TStateMachine, TStateEnum> :
        MonoBehaviour
        where TStateMachine : StateMachine<TStateEnum>
        where TStateEnum : struct, IConvertible
    {
        [SerializeField]
        private TStateMachine _StateMachine;

        public TStateMachine StateMachine
        {
            get { return _StateMachine; }
            protected set { _StateMachine = value; }
        }

        private SceneHandlerGroup _CurrentLoadGroup;

        protected SceneHandlerGroup CurrentLoadGroup
        {
            get { return _CurrentLoadGroup; }
            private set { _CurrentLoadGroup = value; }
        }

        private SceneHandlerGroup _CurrentUnloadGroup;

        protected SceneHandlerGroup CurrentUnloadGroup
        {
            get { return _CurrentUnloadGroup; }
            private set { _CurrentUnloadGroup = value; }
        }

        private void HandleBeforeStateChange(object sender)
        {
            CurrentLoadGroup = new SceneHandlerGroup();
            CurrentUnloadGroup = new SceneHandlerGroup();
        }

        private void HandleAfterStateChange(object sender)
        {
            if (this.isActiveAndEnabled)
            {
                CurrentLoadGroup.StartLoadAsync(this);
                CurrentUnloadGroup.StartUnloadAsync(this);
            }
        }

        protected virtual void ConfigureForStateMachine(TStateMachine state_machine)
        {
            state_machine.BeforeStateChange.RemAddListenerAuto(HandleBeforeStateChange);
            state_machine.AfterStateChange.RemAddListenerAuto(HandleAfterStateChange);
        }

        [ContextMenu("Configure with State Machine")]
        public void Configure()
        {
            StateMachine = StateMachine ?? GetComponent<TStateMachine>();
            if (StateMachine == null)
                throw new ArgumentNullException("Scene control state machine is missing");
            ConfigureForStateMachine(StateMachine);
        }

        private void Start()
        {
        }
    }
}