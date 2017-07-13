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

namespace RoaringFangs.GUI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public class VirtualMonospaceText :
        MonoBehaviour,
        ISerializationCallbackReceiver
    {
        [SerializeField]
        private UnityEngine.UI.Text _Self;

        [SerializeField]
        private UnityEngine.UI.Text[] _Digits;

        private void Update()
        {
            var padded = _Self.text.PadLeft(_Digits.Length);
            var self_color = _Self.color;
            for (var i = 0; i < _Digits.Length; i++)
            {
                var digit = _Digits[i];
                digit.text = padded[i].ToString();
                digit.color = self_color;
            }
        }

        public void OnBeforeSerialize()
        {
            _Self = GetComponent<UnityEngine.UI.Text>();
            _Self.enabled = false;
            _Digits = GetComponentsInChildren<UnityEngine.UI.Text>()
                .Where(t => t != _Self)
                .ToArray();
        }

        public void OnAfterDeserialize()
        {
        }
    }
}