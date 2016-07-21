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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoaringFangs.UserInput
{
    public class InputMap : MonoBehaviour
    {
        [System.Serializable]
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

        [System.Serializable]
        public struct StringRule
        {
            [SerializeField]
            private string _Name;

            public string Name
            {
                get { return _Name; }
                set { _Name = value; }
            }

            [SerializeField]
            private bool _Bypass;

            public bool Bypass
            {
                get { return _Bypass; }
                set { _Bypass = value; }
            }

            [SerializeField]
            private InputSelection _Source;

            public InputSelection Source
            {
                get { return _Source; }
                set { _Source = value; }
            }

            public StringRule(InputSelection src, string dst)
            {
                _Bypass = true;
                _Name = dst;
                _Source = src;
            }
        }

        [System.Serializable]
        public struct IntRule
        {
            [SerializeField]
            private bool _Bypass;

            public bool Bypass
            {
                get { return _Bypass; }
                set { _Bypass = value; }
            }

            [SerializeField]
            private int _Id;

            public int Id
            {
                get { return _Id; }
                set { _Id = value; }
            }

            [SerializeField]
            private InputSelection _Source;

            public InputSelection Source
            {
                get { return _Source; }
                set { _Source = value; }
            }

            public IntRule(InputSelection src, int dst)
            {
                _Bypass = true;
                _Id = dst;
                _Source = src;
            }
        }

        public enum InputType
        {
            Axis,
            Button,
            Key,
            MouseButton
        }

        public StringRule[] AxisInputMapping;
        public StringRule[] ButtonInputMapping;
        public StringRule[] KeyInputMapping;
        public IntRule[] MouseButtonInputMapping;

        private Dictionary<string, InputSelection> _AxisInputMappingDictionary = new Dictionary<string, InputSelection>();
        private Dictionary<string, InputSelection> _ButtonInputMappingDictionary = new Dictionary<string, InputSelection>();
        private Dictionary<string, InputSelection> _KeyInputMappingDictionary = new Dictionary<string, InputSelection>();
        private Dictionary<int, InputSelection> _MouseButtonInputMappingDictionary = new Dictionary<int, InputSelection>();

        private IEnumerable<StringRule> EnabledRules(IEnumerable<StringRule> rules)
        {
            foreach (var rule in rules)
            {
                if (!rule.Bypass)
                    yield return rule;
            }
        }

        private IEnumerable<IntRule> EnabledRules(IEnumerable<IntRule> rules)
        {
            foreach (var rule in rules)
            {
                if (!rule.Bypass)
                    yield return rule;
            }
        }

        private void Start()
        {
            CollectInputMappingDictionaries();
        }

        private void OnValidate()
        {
            CollectInputMappingDictionaries();
        }

        private void CollectInputMappingDictionaries()
        {
            _AxisInputMappingDictionary.Clear();
            foreach (StringRule e in EnabledRules(AxisInputMapping))
                _AxisInputMappingDictionary[e.Name] = e.Source;

            _ButtonInputMappingDictionary.Clear();
            foreach (var e in EnabledRules(ButtonInputMapping))
                _ButtonInputMappingDictionary[e.Name] = e.Source;

            _KeyInputMappingDictionary.Clear();
            foreach (var e in EnabledRules(KeyInputMapping))
                _KeyInputMappingDictionary[e.Name] = e.Source;

            _MouseButtonInputMappingDictionary.Clear();
            foreach (var e in EnabledRules(MouseButtonInputMapping))
                _MouseButtonInputMappingDictionary[e.Id] = e.Source;
        }

        public void CollectInputMappingArray()
        {
            AxisInputMapping = _AxisInputMappingDictionary.Select(e => new StringRule(e.Value, e.Key)).ToArray();
        }

        public float GetAxis(string name)
        {
            InputSelection d;
            if (_AxisInputMappingDictionary.TryGetValue(name, out d))
                return d.GetFloatValueAuto(Input.GetAxis, Input.GetButton, Input.GetKey, Input.GetMouseButton);
            else
                return Input.GetAxis(name);
        }

        public float GetAxisRaw(string name)
        {
            InputSelection d;
            if (_AxisInputMappingDictionary.TryGetValue(name, out d))
                return d.GetFloatValueAuto(Input.GetAxisRaw, Input.GetButton, Input.GetKey, Input.GetMouseButton);
            else
                return Input.GetAxisRaw(name);
        }

        public bool GetButton(string name)
        {
            InputSelection d;
            if (_ButtonInputMappingDictionary.TryGetValue(name, out d))
                return d.GetBoolValueAuto(Input.GetAxis, Input.GetButton, Input.GetKey, Input.GetMouseButton);
            else
                return Input.GetButton(name);
        }

        public bool GetButtonDown(string name)
        {
            InputSelection d;
            if (_ButtonInputMappingDictionary.TryGetValue(name, out d))
                return d.GetBoolValueAuto(Input.GetAxis, Input.GetButtonDown, Input.GetKeyDown, Input.GetMouseButtonDown);
            else
                return Input.GetButtonDown(name);
        }

        public bool GetButtonUp(string name)
        {
            InputSelection d;
            if (_ButtonInputMappingDictionary.TryGetValue(name, out d))
                return d.GetBoolValueAuto(Input.GetAxis, Input.GetButtonUp, Input.GetKeyUp, Input.GetMouseButtonUp);
            else
                return Input.GetButtonUp(name);
        }

        public bool GetKey(string name)
        {
            InputSelection d;
            if (_KeyInputMappingDictionary.TryGetValue(name, out d))
                return d.GetBoolValueAuto(Input.GetAxis, Input.GetButton, Input.GetKey, Input.GetMouseButton);
            else
                return Input.GetKey(name);
        }

        public bool GetKeyDown(string name)
        {
            InputSelection d;
            if (_KeyInputMappingDictionary.TryGetValue(name, out d))
                return d.GetBoolValueAuto(Input.GetAxis, Input.GetButtonDown, Input.GetKeyDown, Input.GetMouseButtonDown);
            else
                return Input.GetKeyDown(name);
        }

        public bool GetKeyUp(string name)
        {
            InputSelection d;
            if (_KeyInputMappingDictionary.TryGetValue(name, out d))
                return d.GetBoolValueAuto(Input.GetAxis, Input.GetButtonUp, Input.GetKeyUp, Input.GetMouseButtonUp);
            else
                return Input.GetKeyUp(name);
        }

        public bool GetMouseButton(int index)
        {
            InputSelection d;
            if (_MouseButtonInputMappingDictionary.TryGetValue(index, out d))
                return d.GetBoolValueAuto(Input.GetAxis, Input.GetButton, Input.GetKey, Input.GetMouseButton);
            else
                return Input.GetMouseButton(index);
        }

        public bool GetMouseButtonDown(int index)
        {
            InputSelection d;
            if (_MouseButtonInputMappingDictionary.TryGetValue(index, out d))
                return d.GetBoolValueAuto(Input.GetAxis, Input.GetButtonDown, Input.GetKeyDown, Input.GetMouseButtonDown);
            else
                return Input.GetMouseButtonDown(index);
        }

        public bool GetMouseButtonUp(int index)
        {
            InputSelection d;
            if (_MouseButtonInputMappingDictionary.TryGetValue(index, out d))
                return d.GetBoolValueAuto(Input.GetAxis, Input.GetButtonUp, Input.GetKeyUp, Input.GetMouseButtonUp);
            else
                return Input.GetMouseButtonUp(index);
        }
    }
}