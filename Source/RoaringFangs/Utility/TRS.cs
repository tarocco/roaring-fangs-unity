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
    public struct TRS
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public static TRS Create(ref Matrix4x4 m)
        {
            return new TRS()
            {
                Position = GetPosition(ref m),
                Rotation = GetRotation(ref m),
                Scale = GetScale(ref m)
            };
        }

        public static Vector3 GetPosition(ref Matrix4x4 m)
        {
            return m.GetColumn(3);
        }

        public static Quaternion GetRotation(ref Matrix4x4 m)
        {
            return Quaternion.LookRotation(
                    m.GetColumn(2),
                    m.GetColumn(1));
        }

        public static Vector3 GetScale(ref Matrix4x4 m)
        {
            return new Vector3(
                    m.GetColumn(0).magnitude,
                    m.GetColumn(1).magnitude,
                    m.GetColumn(2).magnitude);
        }
    }
}