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

namespace RoaringFangs.FSM
{
    public class StateInfo
    {
        [Serializable]
        public struct Surrogate
        {
            public ActionSurrogate EntryAction, BodyAction, ExitAction;

            public static implicit operator StateInfo(Surrogate self)
            {
                return new StateInfo(
                    self.EntryAction,
                    self.BodyAction,
                    self.ExitAction);
            }

            public Surrogate(
                Action entry_action,
                Action body_action,
                Action exit_action)
            {
                EntryAction = entry_action;
                BodyAction = body_action;
                ExitAction = exit_action;
            }
        }

        public static implicit operator Surrogate(StateInfo self)
        {
            return new Surrogate(
                self.EntryAction,
                self.BodyAction,
                self.ExitAction);
        }

        private Action _EntryAction;

        public Action EntryAction
        {
            get { return _EntryAction; }
            set { _EntryAction = value; }
        }

        private Action _BodyAction = null;

        public Action BodyAction
        {
            get { return _BodyAction; }
            set { _BodyAction = value; }
        }

        private Action _ExitAction = null;

        public Action ExitAction
        {
            get { return _ExitAction; }
            set { _ExitAction = value; }
        }

        public StateInfo(
            Action on_enter,
            Action while_in,
            Action on_exit)
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