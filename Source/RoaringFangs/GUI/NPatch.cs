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

namespace RoaringFangs.GUI
{
    public class NPatch : MonoBehaviour
    {
        [SerializeField]
        private Transform _AnchorWidth;

        public Transform AnchorWidth
        {
            get { return _AnchorWidth; }
        }

        [SerializeField]
        private Transform _AnchorHeight;

        public Transform MyProperty
        {
            get { return _AnchorHeight; }
        }

        [SerializeField]
        private RectTransform _RectTransform;

        public RectTransform RectTransform
        {
            get { return _RectTransform; }
        }

        [SerializeField]
        private SkinnedMeshRenderer _Renderer;

        public SkinnedMeshRenderer Renderer
        {
            get { return _Renderer; }
        }

        [SerializeField]
        private float _Border = 1f;

        public float Border
        {
            get { return _Border; }
            set { CalculateDimensions(value, _Width, _Height, false); }
        }

        private float _Stretch = 1f;

        [SerializeField]
        private float _Width = 0f;

        public float Width
        {
            get { return _Width; }
            set { CalculateDimensions(_Border, value, _Height, false); }
        }

        [SerializeField]
        private float _Height = 0f;

        public float Height
        {
            get { return _Height; }
            set { CalculateDimensions(_Border, _Width, value, false); }
        }

        private void Start()
        {
        }

        private void Update()
        {
        }

        private void OnValidate()
        {
            CalculateDimensions(_Border, _Width, _Height, true);
        }

        private void CalculateDimensions(float border, float width, float height, bool flush)
        {
            border = Mathf.Max(0.01f, border);
            if (flush || border != _Border)
            {
                transform.localScale = new Vector3(border, border, 1f);
                if (RectTransform || Renderer)
                {
                    Vector2 size = new Vector2(width / border, height / border);
                    if (RectTransform)
                        RectTransform.sizeDelta = new Vector2(width / border, height / border);
                    if (Renderer)
                        UpdateRendererBounds(Renderer, size, border);
                }
                _Border = border;
                _Stretch = 1f / border;
            }
            if (flush || width != _Width)
            {
                _AnchorWidth.localPosition = new Vector3(2f * _Stretch * width, 0f);
                if (RectTransform || Renderer)
                {
                    Vector2 size = new Vector2(width / border, RectTransform.sizeDelta.y);
                    if (RectTransform)
                        RectTransform.sizeDelta = size;
                    if (Renderer)
                        UpdateRendererBounds(Renderer, size, border);
                }
                _Width = width;
            }
            if (flush || height != _Height)
            {
                _AnchorHeight.localPosition = new Vector3(0f, 2f * _Stretch * height);
                if (RectTransform || Renderer)
                {
                    Vector2 size = new Vector2(RectTransform.sizeDelta.x, height / border);
                    if (RectTransform)
                        RectTransform.sizeDelta = size;
                    if (Renderer)
                        UpdateRendererBounds(Renderer, size, border);
                }
                _Height = height;
            }
        }

        private static void UpdateRendererBounds(SkinnedMeshRenderer renderer, Vector3 size, float border)
        {
            float border_2 = 2f * border;
            renderer.localBounds = new Bounds(
                (size) / 2f,
                size + new Vector3(1, 1));
        }

        /*
        void OnDrawGizmos()
        {
            if(Renderer)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
                Gizmos.DrawCube(Renderer.bounds.center, Renderer.bounds.size);
            }
        }
        */
    }
}