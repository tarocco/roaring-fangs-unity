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

namespace RoaringFangs.GSR
{
    public interface ITexturable<TTexture> where TTexture : Texture
    {
        TTexture Texture { get; set; }
    }

    public interface ITexturable2<TTexture> : ITexturable<TTexture> where TTexture : Texture
    {
        TTexture Texture1 { get; set; }
    }

    public interface ITexturable3<TTexture> : ITexturable2<TTexture> where TTexture : Texture
    {
        TTexture Texture2 { get; set; }
    }

    public interface ITexturable4<TTexture> : ITexturable3<TTexture> where TTexture : Texture
    {
        TTexture Texture3 { get; set; }
    }
}