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
using UnityEngine.UI;

namespace RoaringFangs.GUI
{
    [RequireComponent(typeof(GUIText))]
    [ExecuteInEditMode]

    public class SyncText : MonoBehaviour, ISerializationCallbackReceiver
    {
        public Text Source;
        public Text[] Destinations = {};

        public bool
            ApplyText = true,
            ApplyFont = true,
            ApplyFontSize = true,
            ApplyFontStyle = true;

        private static void ForEach<T>(IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var e in enumerable)
                action(e);
        }

        private void Start()
        {
        }

        private void Update()
        {
            var destinations = Destinations.Where(t => t != null).ToArray();
            if (ApplyText)
                ForEach(destinations, e => e.text = Source.text);
            if (ApplyFont)
                ForEach(destinations, e => e.font = Source.font);
            if (ApplyFontSize)
                ForEach(destinations, e => e.fontSize = Source.fontSize);
            if (ApplyFontStyle)
                ForEach(destinations, e => e.fontStyle = Source.fontStyle);
        }

        public void OnBeforeSerialize()
        {
            if (Source == null)
                Source = GetComponent<Text>();
        }

        public void OnAfterDeserialize()
        {
        }

#if UNITY_EDITOR

        [ContextMenu("Add all children as destinations")]
        public void AddAllChildrenAsDestinations()
        {
            var children = GetComponentsInChildren<Text>();
            Destinations = Destinations
                .Concat(children)
                .Except(new[] {Source})
                .Distinct()
                .ToArray();
        }

#endif
    }
}