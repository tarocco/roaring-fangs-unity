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

using RoaringFangs.Attributes;
using RoaringFangs.Editor;
using UnityEngine;

namespace RoaringFangs.GSR
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(BlitFx))]
    public class PreBlitFx : BlitFxBase, ITexturable<RenderTexture>, ISerializationCallbackReceiver
    {
        [SerializeField, AutoProperty]
        private RenderTexture _Texture;

        private RenderTexture _BufferTexture;
        public RenderTexture Texture
        {
            get { return _Texture; }
            set
            {
                _Texture = value;
                if(_BufferTexture != null)
                    _BufferTexture.Release();
                if (value != null)
                {
                    if(
                        _BufferTexture == null ||
                        _BufferTexture.width != value.width ||
                        _BufferTexture.height != value.height ||
                        _BufferTexture.depth != value.depth ||
                        _BufferTexture.format != value.format)
                    _BufferTexture = new RenderTexture(value.width, value.height, value.depth, value.format,
                        RenderTextureReadWrite.Default);
                }
            }
        }

        public RenderTexture GetBufferedCopyOfTexture()
        {
            Graphics.Blit(Texture, _BufferTexture);
            return _BufferTexture;
        }

        private void Start()
        {
        }

        public void OnBeforeSerialize()
        {
            EditorUtilities.OnBeforeSerializeAutoProperties(this);
        }

        public void OnAfterDeserialize()
        {
        }
    }
}