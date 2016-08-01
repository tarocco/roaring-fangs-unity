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

using RoaringFangs.Utility;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace RoaringFangs.FSM
{
    [Serializable]
    public class StateInfo
    {
        [Serializable]
        public class Event : UnityEvent<UnityEngine.Object>
        {
        }

        [Serializable]
        public struct Surrogate
        {
            public ActionSurrogate EntryAction, BodyAction, ExitAction;
        }

        [SerializeField]
        private Action<UnityEngine.Object> _EntryAction;
        public Action<UnityEngine.Object> EntryAction
        {
            get { return _EntryAction; }
            set { _EntryAction = value; }
        }

        [SerializeField]
        private Action<UnityEngine.Object> _BodyAction = null;
        public Action<UnityEngine.Object> BodyAction
        {
            get { return _BodyAction; }
            set { _BodyAction = value; }
        }

        [SerializeField]
        private Action<UnityEngine.Object> _ExitAction = null;
        public Action<UnityEngine.Object> ExitAction
        {
            get { return _ExitAction; }
            set { _ExitAction = value; }
        }

        
        public static StateInfo FromSurrogate(Surrogate surrogate)
        {
            return new StateInfo(
                surrogate.EntryAction.ToAction<UnityEngine.Object>(),
                surrogate.BodyAction.ToAction<UnityEngine.Object>(),
                surrogate.ExitAction.ToAction<UnityEngine.Object>());
        }

        public Surrogate ToSurrogate()
        {
            Surrogate surrogate = new Surrogate();
            if(EntryAction != null)
                surrogate.EntryAction = ActionSurrogate.FromAction(EntryAction);
            if (BodyAction != null)
                surrogate.BodyAction = ActionSurrogate.FromAction(BodyAction);
            if(ExitAction != null)
                surrogate.ExitAction = ActionSurrogate.FromAction(ExitAction);
            return surrogate;
        }

        public StateInfo(
            Action<UnityEngine.Object> on_enter,
            Action<UnityEngine.Object> while_in,
            Action<UnityEngine.Object> on_exit)
        {
            EntryAction = on_enter;
            BodyAction = while_in;
            ExitAction = on_exit;
        }

        public StateInfo() :
            this(null, null, null)
        {
        }
    }
}