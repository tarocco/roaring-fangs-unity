using RoaringFangs.Attributes;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace RoaringFangs.Animation
{
    public class RefBoolBehavior : RefBoolBehaviorBase
    {
        //public class ValueChangedEventArgs : EventArgs
        //{
        //    public readonly bool Value;
        //    public ValueChangedEventArgs(bool value)
        //    {
        //        Value = value;
        //    }
        //}

        //[Serializable]
        //public class ValueChangedEvent : UnityEvent<object, ValueChangedEventArgs> { }

        public override bool? Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                 base.Value = value;
                 //_ValueChanged.Invoke(this, new ValueChangedEventArgs(value.Value));
            }
        }

        //[SerializeField]
        //private ValueChangedEvent _ValueChanged;

        //public ValueChangedEvent ValueChanged
        //{
        //    get { return _ValueChanged; }
        //    protected set { _ValueChanged = value; }
        //}
    }
}