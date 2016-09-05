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

        public TRS(ref Matrix4x4 m)
        {
            Position = GetPosition(ref m);
            Rotation = GetRotation(ref m);
            Scale = GetScale(ref m);
        }

        public TRS(Matrix4x4 m)
        {
            Position = GetPosition(ref m);
            Rotation = GetRotation(ref m);
            Scale = GetScale(ref m);
        }

        public static Vector3 GetPosition(ref Matrix4x4 m)
        {
            return m.GetColumn(3);
        }

        public static Vector3 GetPosition(Matrix4x4 m)
        {
            return GetPosition(ref m);
        }

        public static Quaternion GetRotation(ref Matrix4x4 m)
        {
            return Quaternion.LookRotation(
                    m.GetColumn(2),
                    m.GetColumn(1));

            // This is an attempt at a more accurate method but there's a typo in here I think -TB
            //float w_unscaled = Mathf.Sqrt(1f + m[0, 0] + m[1, 1] + m[2, 2]);
            //Vector4 v4 = new Vector4()
            //{
            //    w = 0.5f * w_unscaled,
            //    x = (m[2, 1] - m[1, 2]) / (2f * w_unscaled),
            //    y = (m[0, 2] - m[2, 0]) / (2f * w_unscaled),
            //    z = (m[1, 0] - m[0, 1]) / (2f * w_unscaled),
            //};
            //v4.Normalize();
            //return new Quaternion(v4.x, v4.y, v4.z, v4.w);
        }

        public static Quaternion GetRotation(Matrix4x4 m)
        {
            return GetRotation(ref m);
        }

        public static Vector3 GetScale(ref Matrix4x4 m)
        {
            return new Vector3(
                    m.GetColumn(0).magnitude,
                    m.GetColumn(1).magnitude,
                    m.GetColumn(2).magnitude);
        }

        public static Vector3 GetScale(Matrix4x4 m)
        {
            return GetScale(ref m);
        }

        public Matrix4x4 ToMatrix()
        {
            return Matrix4x4.TRS(Position, Rotation, Scale);
        }
    }
}