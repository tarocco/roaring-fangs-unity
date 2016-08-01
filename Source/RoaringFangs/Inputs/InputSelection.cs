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

namespace RoaringFangs.Inputs
{
    [Serializable]
    public struct InputSelection
    {
        public InputType Type;
        public string StringValue;
        public int IntValue;

        public T GetByStringValue<T>(System.Func<string, T> f)
        {
            return f(StringValue);
        }

        public T GetByIntValue<T>(System.Func<int, T> f)
        {
            return f(IntValue);
        }

        // string: bool to float
        public float GetFloatValue(System.Func<string, bool> f)
        {
            return GetByStringValue(f) ? 1f : 0f;
        }

        // string: float to bool
        public bool GetBoolValue(System.Func<string, float> f)
        {
            return GetByStringValue(f) > 0f;
        }

        // int: bool to float
        public float GetFloatValue(System.Func<int, bool> f)
        {
            return GetByIntValue(f) ? 1f : 0f;
        }

        public float GetFloatValueAuto(
            System.Func<string, float> f_axis,
            System.Func<string, bool> f_button,
            System.Func<string, bool> f_key,
            System.Func<int, bool> f_mouse_button)
        {
            switch (Type)
            {
                default:
                case InputType.Axis:
                    return GetByStringValue(f_axis);

                case InputType.Button:
                    return GetFloatValue(f_button);

                case InputType.Key:
                    return GetFloatValue(f_key);

                case InputType.MouseButton:
                    return GetFloatValue(f_mouse_button);
            }
        }

        public bool GetBoolValueAuto(
            System.Func<string, float> f_axis,
            System.Func<string, bool> f_button,
            System.Func<string, bool> f_key,
            System.Func<int, bool> f_mouse_button)
        {
            switch (Type)
            {
                default:
                case InputType.Axis:
                    return GetBoolValue(f_axis);

                case InputType.Button:
                    return GetByStringValue(f_button);

                case InputType.Key:
                    return GetByStringValue(f_key);

                case InputType.MouseButton:
                    return GetByIntValue(f_mouse_button);
            }
        }
    }
}