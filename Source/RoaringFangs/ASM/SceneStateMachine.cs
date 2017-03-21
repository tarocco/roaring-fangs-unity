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
    public class SceneStateMachine : SceneStateBase
    {
        public override void OnManagedStateMachineVerifyEnter(
            ControlledStateManager manager,
            ManagedStateMachineEventArgs args)
        {
            DoVerifyEnter();
        }

        public override void OnManagedStateMachineEnter(
            ControlledStateManager manager,
            ManagedStateMachineEventArgs args)
        {
            DoEnter(manager);
        }

        public override void OnManagedStateMachineVerifyExit(
            ControlledStateManager manager,
            ManagedStateMachineEventArgs args)
        {
            DoVerifyExit();
        }

        public override void OnManagedStateMachineExit(
            ControlledStateManager manager,
            ManagedStateMachineEventArgs args)
        {
            DoExit(manager);
        }

        public override void OnManagedStateVerifyEnter(
            ControlledStateManager manager,
            ManagedStateEventArgs args)
        {
            if(manager.AcceptStateMachineStateEvents)
                DoVerifyEnter();
        }

        public override void OnManagedStateEnter(
            ControlledStateManager manager,
            ManagedStateEventArgs args)
        {
            if (manager.AcceptStateMachineStateEvents)
                DoEnter(manager);
        }

        public override void OnManagedStateVerifyExit(
            ControlledStateManager manager,
            ManagedStateEventArgs args)
        {
            if (manager.AcceptStateMachineStateEvents)
                DoVerifyExit();
        }

        public override void OnManagedStateExit(
            ControlledStateManager manager,
            ManagedStateEventArgs args)
        {
            if (manager.AcceptStateMachineStateEvents)
                DoExit(manager);
        }
    }
}