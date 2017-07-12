using RoaringFangs.Attributes;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace RoaringFangs.Utility
{
    public class PropertyChangedEventArgs<T> : EventArgs
    {
        public readonly T PreviousValue;
        public readonly T CurrentValue;

        public PropertyChangedEventArgs(
            T previous_value,
            T current_value)
        {
            PreviousValue = previous_value;
            CurrentValue = current_value;
        }
    }

    /*
    public class FloatPropertyChangedEventArgs : PropertyChangedEventArgs<float>
    {
        public FloatPropertyChangedEventArgs(
            float previous_value,
            float current_value) :
        base(previous_value, current_value)
        {
        }
    }

    public class IntPropertyChangedEventArgs : PropertyChangedEventArgs<int>
    {
        IntPropertyChangedEventArgs(
            int previous_value,
            int current_value) :
        base(previous_value, current_value)
        {
        }
    }
    */

    [Serializable]
    public class FloatPropertyChangedEvent : UnityEvent<object, PropertyChangedEventArgs<float>> { }

    [Serializable]
    public class IntPropertyChangedEvent : UnityEvent<object, PropertyChangedEventArgs<int>> { }

    [Serializable]
    public class BoolPropertyChangedEvent : UnityEvent<object, PropertyChangedEventArgs<bool>> { }
}