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

using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RoaringFangs.GUI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Text))]
    public class VirtualMonospaceText :
        MonoBehaviour,
        ISerializationCallbackReceiver
    {
        [SerializeField]
        private Text _Self;

        private Text Self
        {
            get
            {
                // TODO: find out why this causes problems in OnBeforeSerialize
                if (_Self == null)
                    _Self = GetComponent<Text>();
                if (_Self != null)
                    _Self.enabled = false;
                return _Self;
            }
            set { _Self = value; }
        }

        [SerializeField]
        private Text[] _Digits = { };

        private void Update()
        {
            var padded = Self.text.PadLeft(_Digits.Length);
            var self_color = Self.color;
            for (var i = 0; i < _Digits.Length; i++)
            {
                var digit = _Digits[i];
                digit.text = padded[i].ToString();
                digit.color = self_color;
            }
        }

        public void OnBeforeSerialize()
        {
            _Digits = GetComponentsInChildren<Text>()
                .Where(t => t != _Self)
                .ToArray();
        }

        public void OnAfterDeserialize()
        {

        }
    }
}