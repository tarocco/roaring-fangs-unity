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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        protected struct SceneHandlerGroupPair
        {
            public SceneHandlerGroup LoadGroup;
            public SceneHandlerGroup UnloadGroup;
        }

        private Stack<SceneHandlerGroupPair> _SceneHandlerGroupPairs;

        protected Stack<SceneHandlerGroupPair> SceneHandlerGroupPairs
        {
            get
            {
                if (_SceneHandlerGroupPairs == null)
                    _SceneHandlerGroupPairs = new Stack<SceneHandlerGroupPair>();
                return _SceneHandlerGroupPairs;
            }
            private set { _SceneHandlerGroupPairs = value; }
        }

        protected SceneHandlerGroupPair CurrentSceneHandlerGroupPair
        {
            get { return _SceneHandlerGroupPairs.Peek(); }
        }

        public SceneHandlerGroup CurrentLoadGroup
        {
            get { return CurrentSceneHandlerGroupPair.LoadGroup; }
        }

        public SceneHandlerGroup CurrentUnloadGroup
        {
            get { return CurrentSceneHandlerGroupPair.UnloadGroup; }
        }

        private void HandleBeforeStateChange(object sender)
        {
            //if (isActiveAndEnabled)
            {
                var pair = new SceneHandlerGroupPair()
                {
                    LoadGroup = new SceneHandlerGroup(),
                    UnloadGroup = new SceneHandlerGroup(),
                };
                SceneHandlerGroupPairs.Push(pair);
            }
        }

        private void HandleAfterStateChange(object sender)
        {
            if (isActiveAndEnabled)
            {
                var pair = SceneHandlerGroupPairs.Pop();
                pair.LoadGroup.StartLoadAsync(this);
                pair.UnloadGroup.StartUnloadAsync(this);
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
    }
}
