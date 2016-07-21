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

namespace RoaringFangs.Utility
{
    public static partial class Assets
    {
        #region File Management

        public static string StreamingAssetsPathCombine(params string[] elements)
        {
            string[] elements_trimmed = new string[elements.Length];
            for (int i = 0; i < elements_trimmed.Length; i++)
                elements_trimmed[i] = elements[i].Trim('/', '\\');
            string path_joined = Application.streamingAssetsPath + "/" + string.Join("/", elements_trimmed);
            string path_uri_fixed = new System.Uri(path_joined).ToString().Replace("\\", "/");
            return path_uri_fixed;
        }

        #endregion File Management
    }
}