using RoaringFangs.Attributes;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace RoaringFangs.Utility
{
    public class PropertyEventArgs<T> : EventArgs
    {
        public readonly T PreviousValue;
        public readonly T CurrentValue;

        public PropertyEventArgs(
            T previous_value,
            T current_value)
        {
            PreviousValue = previous_value;
            CurrentValue = current_value;
        }
    }

    public class PropertyEvent<T> : UnityEvent<object, PropertyEventArgs<T>> { }

    [Serializable]
    public class FloatPropertyEvent : PropertyEvent<float> { }

    [Serializable]
    public class IntPropertyEvent : PropertyEvent<int> { }

    [Serializable]
    public class BoolPropertyEvent : PropertyEvent<bool> { }
}