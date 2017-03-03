using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RoaringFangs.Utility;
using UnityEngine.Events;

namespace RoaringFangs
{
    public class NavigationStack : MonoBehaviour
    {
        public enum NavigationType
        {
            Push,
            Pop
        }
        [Serializable]
        public class NavigationEventArgs : EventArgs
        {
            
            public readonly string Current;
            public readonly string Next;
            public readonly NavigationType Type;
            public NavigationEventArgs(string current, string next, NavigationType type)
            {
                Current = current;
                Next = next;
                Type = type;
            }
        }

        [Serializable]
        public class NavigationEvent : UnityEvent<object, NavigationEventArgs> { }

        [SerializeField]
        private NavigationEvent _Navigation;

        public NavigationEvent Navigation
        {
            get { return _Navigation; }
        }

        private Stack<string> _Stack = new Stack<string>();
        protected Stack<string> Stack
        {
            get { return _Stack; }
        }

        public string Current
        {
            get { return Stack.Peek(); }
        }

        public string PopNavigation()
        {
            string current = Stack.Pop();
            string next = Stack.Peek();
            Navigation.Invoke(this, new NavigationEventArgs(current, next, NavigationType.Pop));
            return current;
        }

        public void PushNavigation(string next)
        {
            string current = Stack.Pop();
            Stack.Push(next);
            Navigation.Invoke(this, new NavigationEventArgs(current, next, NavigationType.Push));
        }

        void Start()
        {
        }
    }
}