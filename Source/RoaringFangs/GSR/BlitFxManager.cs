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
using RoaringFangs.Attributes;
using UnityEngine;

namespace RoaringFangs.GSR
{
    [ExecuteInEditMode]
    public class BlitFxManager : BlitFxManagerBase
    {
        #region Updaters

        [SerializeField, AutoProperty]
        private UpdateDirectiveTexture[] _UpdateTextures = {};

        public UpdateDirectiveTexture[] UpdateTextures
        {
            get { return _UpdateTextures; }
            private set { _UpdateTextures = value; }
        }

        [SerializeField, AutoProperty]
        private UpdateDirectiveColor[] _UpdateColors = {};

        public UpdateDirectiveColor[] UpdateColors
        {
            get { return _UpdateColors; }
            private set { _UpdateColors = value; }
        }

        [SerializeField, AutoProperty]
        private UpdateDirectiveFloat[] _UpdateFloats = {};

        public UpdateDirectiveFloat[] UpdateFloats
        {
            get { return _UpdateFloats; }
            private set { _UpdateFloats = value; }
        }

        #endregion Updaters

        public override IEnumerable<UpdateDirectiveBase> Directives()
        {
            foreach (var directive in UpdateFloats)
                yield return directive;
            foreach (var directive in UpdateColors)
                yield return directive;
            foreach (var directive in UpdateTextures)
                yield return directive;
        }

        private void OnPreRender()
        {
            foreach (var updater in Directives())
                updater.ApplyAll();
        }
    }
}