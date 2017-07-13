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

    public class PropertyChangedEvent<T> : UnityEvent<object, PropertyChangedEventArgs<T>> { }

    [Serializable]
    public class FloatPropertyChangedEvent : PropertyChangedEvent<float> { }

    [Serializable]
    public class IntPropertyChangedEvent : PropertyChangedEvent<int> { }

    [Serializable]
    public class BoolPropertyChangedEvent : PropertyChangedEvent<bool> { }
}