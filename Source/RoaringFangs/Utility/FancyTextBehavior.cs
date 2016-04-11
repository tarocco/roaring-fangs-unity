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

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

namespace RoaringFangs.Utility
{
    [ExecuteInEditMode]
    [System.Obsolete("This thing sucks. Make it suck less?")]
    public class FancyTextBehavior : MonoBehaviour
    {
        // >> WET Model: ManualSortingOrder
        public string SortingLayerName = "Default";
        public int SortingOrder = 0;
        // << WET Model: ManualSortingOrder
        private float _Opacity = float.NaN;
        public float Opacity = 1.0f;
        void Start()
        {
            // >> WET Model: ManualSortingOrder
            if (GetComponent<Renderer>() != null)
            {
                GetComponent<Renderer>().sortingLayerName = SortingLayerName;
                GetComponent<Renderer>().sortingOrder = SortingOrder;
            }
            // << WET Model: ManualSortingOrder
        }
        void Update()
        {
            // >> WET Model: ManualSortingOrder
            if (GetComponent<Renderer>() != null &&
               (GetComponent<Renderer>().sortingOrder != SortingOrder ||
                GetComponent<Renderer>().sortingLayerName != SortingLayerName))
            {
                GetComponent<Renderer>().sortingLayerName = SortingLayerName;
                GetComponent<Renderer>().sortingOrder = SortingOrder;
            }
            // << WET Model: ManualSortingOrder
            if (Application.isPlaying && Opacity != _Opacity)
            {
                var color = GetComponent<Renderer>().material.color;
                color.a = _Opacity = Opacity;
                GetComponent<Renderer>().material.color = color;
            }
        }
        public void SetText(string text)
        {
            this.GetComponent<TextMesh>().text = text;
        }
    }
}