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
using RoaringFangs.Attributes;
using RoaringFangs.GSR;
using UnityEngine;

namespace RoaringFangs.CameraBehavior
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class CameraMimick : MonoBehaviour, ITexturable<RenderTexture>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private Camera _Camera;

        [SerializeField]
        private Camera _Source;

        public Camera Source
        {
            get { return _Source; }
            set { _Source = value; }
        }

        [Serializable]
        public struct PreservationFlags
        {
            public bool Texture;
            public bool CullingMask;
            public bool ClearFlags;
            public bool BackgroundColor;
            public bool Depth;
        }

        public PreservationFlags SettingsToPreserve;

        public RenderTexture Texture
        {
            get
            {
                return _Camera.targetTexture;
            }
            set
            {
                _Camera.targetTexture = value;
            }
        }

        private void OnPreCull()
        {
            if (enabled && _Source)
            {
                var prior_texture = _Camera.targetTexture;
                var prior_culling_mask = _Camera.cullingMask;
                var prior_clear_flags = _Camera.clearFlags;
                var prior_background_color = _Camera.backgroundColor;
                var prior_depth = _Camera.depth;

                _Camera.CopyFrom(_Source);

                if (SettingsToPreserve.Texture)
                    _Camera.targetTexture = prior_texture;
                if (SettingsToPreserve.CullingMask)
                    _Camera.cullingMask = prior_culling_mask;
                if (SettingsToPreserve.ClearFlags)
                    _Camera.clearFlags = prior_clear_flags;
                if (SettingsToPreserve.BackgroundColor)
                    _Camera.backgroundColor = prior_background_color;
                if (SettingsToPreserve.Depth)
                    _Camera.depth = prior_depth;
            }
        }

        private void Start()
        {
        }

        public void OnBeforeSerialize()
        {
            if (_Camera == null)
                _Camera = GetComponent<Camera>();
            if (Source == null && transform.parent != null)
                Source = transform.parent.GetComponentInParent<Camera>();
            //EditorUtilities.OnBeforeSerializeAutoProperties(this);
        }

        public void OnAfterDeserialize()
        {
        }
    }
}