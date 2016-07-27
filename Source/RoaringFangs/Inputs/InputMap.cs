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

namespace RoaringFangs.Inputs
{
    public class InputMap : MonoBehaviour, IInput
    {
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