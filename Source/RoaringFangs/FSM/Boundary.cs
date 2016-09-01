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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityObject = UnityEngine.Object;

namespace RoaringFangs.FSM
{
    [Serializable]
    public class Boundary<TStateEnum> :
        ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<string> _StateNames;

        private List<TStateEnum> _States =
            new List<TStateEnum>();

        public IEnumerable<TStateEnum> States
        {
            get { return _States; }
        }

        public void AddStates(params TStateEnum[] states)
        {
            _States.AddRange(states);
        }

        public void RemoveState(TStateEnum state)
        {
            _States.Remove(state);
        }

        [SerializeField]
        private BoundaryEvent _Entry;
        public BoundaryEvent Entry
        {
            get { return _Entry; }
            protected set { _Entry = value; }
        }

        [SerializeField]
        private BoundaryEvent _Exit;
        public BoundaryEvent Exit
        {
            get { return _Exit; }
            protected set { _Exit = value; }
        }

        public void OnStateEntry(TStateEnum exited, TStateEnum entered)
        {
            bool in_exited = _States.Contains(exited);
            bool in_entered = _States.Contains(entered);
            if(in_exited != in_entered)
            {
                if (in_entered)
                    Entry.Invoke(this);
            }
        }

        public void OnStateExit(TStateEnum exited, TStateEnum entered)
        {
            bool in_exited = _States.Contains(exited);
            bool in_entered = _States.Contains(entered);
            if (in_exited != in_entered)
            {
                if (in_exited)
                    Exit.Invoke(this);
            }
        }

        public void OnBeforeSerialize()
        {
            _StateNames = _States
                .Select(s => Enum.GetName(typeof(TStateEnum), s))
                .ToList();
        }

        public void OnAfterDeserialize()
        {
            _States = _StateNames
                .Select(n => (TStateEnum)Enum.Parse(typeof(TStateEnum), n))
                .ToList();
        }
    }
}
