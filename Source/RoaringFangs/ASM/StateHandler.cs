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

/*
using System;
using UnityEngine;

namespace RoaringFangs.ASM
{
    [Serializable]
    public class StateHandler : IStateHandler, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string _Name;

        [SerializeField, HideInInspector]
        private int _NameHash;

        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                _NameHash = Animator.StringToHash(value);
            }
        }

        public int NameHash
        {
            get { return _NameHash; }
        }


        [SerializeField]
        private string _Tag;

        [SerializeField, HideInInspector]
        private int _TagHash;

        public string Tag
        {
            get { return _Tag; }
            set
            {
                _Tag = value;
                _TagHash = Animator.StringToHash(value);
            }
        }

        public int TagHash
        {
            get { return _TagHash; }
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

        public void OnBeforeSerialize()
        {
            Name = Name;
            Tag = Tag;
        }

        public void OnAfterDeserialize()
        {
            Name = Name;
            Tag = Tag;
        }

        public StateHandler()
        {
            Entry = new StateEvent();
            Update = new StateEvent();
            Exit = new StateEvent();
        }

        public StateHandler(string name, string tag = "") :
            this()
        {
            Name = name;
            Tag = tag;
        }

        public void OnStateEnter(Animator animator, AnimatorStateInfo state_info, int layer_index)
        {
            Entry.Invoke(this, new StateEventArgs(animator, state_info, layer_index));
        }

        public void OnStateUpdate(Animator animator, AnimatorStateInfo state_info, int layer_index)
        {
            Update.Invoke(this, new StateEventArgs(animator, state_info, layer_index));
        }

        public void OnStateExit(Animator animator, AnimatorStateInfo state_info, int layer_index)
        {
            Exit.Invoke(this, new StateEventArgs(animator, state_info, layer_index));
        }
    }
}
*/