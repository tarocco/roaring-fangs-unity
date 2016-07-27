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
using UnityEngine;

namespace RoaringFangs.Utility
{
    public static partial class Math2
    {
        #region Constraint

        public static Vector3 LerpUnconstrained(Vector3 a, Vector3 b, float t)
        {
            return a + (b - a) * t;
        }

        public static Quaternion LimitedRotation(Quaternion q, Quaternion Basis, float max_angle)
        {
            float t = max_angle / Quaternion.Angle(Basis, q);
            return Quaternion.Slerp(Basis, q, t);
        }

        public static Quaternion LimitedRotation(Quaternion q, Quaternion Basis, float max_angle, float guidance)
        {
            float t = Math.Min(max_angle / Quaternion.Angle(Basis, q), 1f) - guidance;
            return Quaternion.Slerp(Basis, q, t);
        }

        public static float TanHf(float x)
        {
            float g = Mathf.Exp(2.0f * x);
            return (g - 1.0f) / (g + 1.0f);
        }

        public static T Clamp<T>(T x, T lo, T hi) where T : IComparable
        {
            return x.CompareTo(lo) > 0 ? (x.CompareTo(hi) < 0 ? x : hi) : lo;
        }

        public static float BoundaryF(float x, float lo, float hi, int curvature = 3)
        {
            float curvature_f2 = curvature * 2;
            float range = hi - lo;
            float x_m = Mathf.Clamp01((x - lo) / range);
            float x_m2 = 2f * x_m - 1f;
            float gx = (Mathf.Pow(x_m2, curvature_f2) - 1f) / curvature_f2;
            float fx = x_m2 / Mathf.Exp(gx);
            float fx_2 = 0.5f * fx + 0.5f;
            //float fx_3 = Mathf.Lerp(x_m, fx_2, Mathf.Pow(1.05f * x_m2, 8) - 0.1f);
            return lo + range * fx_2;
        }

        public static float BoundaryWeighedF(float x, float lo, float hi, int curvature = 3)
        {
            float curvature_f2 = curvature * 2;
            float range = hi - lo;
            float x_m = (x - lo) / range;
            float x_m2 = 2f * x_m - 1f;

            x_m2 = Mathf.Clamp(x_m2 - x_m2 / curvature_f2, -1f, 1f);

            float gx = (Mathf.Pow(x_m2, curvature_f2) - 1f) / curvature_f2;
            float fx = x_m2 / Mathf.Exp(gx);
            float fx_2 = 0.5f * fx + 0.5f;
            //float fx_3 = Mathf.Lerp(x_m, fx_2, Mathf.Pow(1.05f * x_m2, 8) - 0.1f);
            return lo + range * fx_2;
        }

        #endregion Constraint

        #region Vector Space

        public static Vector2 Project(Vector2 a, Vector2 b)
        {
            return (Vector2.Dot(a, b) / b.sqrMagnitude) * b;
        }

        public static Vector3 Project(Vector3 a, Vector3 b)
        {
            return (Vector3.Dot(a, b) / b.sqrMagnitude) * b;
        }

        public static Vector2 Reflect(Vector2 v, Vector2 n)
        {
            return v - (Vector2.Dot(2 * v, n) / n.sqrMagnitude) * n;
        }

        public static Vector3 Reflect(Vector3 v, Vector3 n)
        {
            return v - (Vector3.Dot(2 * v, n) / n.sqrMagnitude) * n;
        }

        public static Vector3 LerpV3(Vector3 a, Vector3 b, Vector3 t)
        {
            return new Vector3(Mathf.Lerp(a.x, b.x, t.x), Mathf.Lerp(a.y, b.y, t.y), Mathf.Lerp(a.z, b.z, t.z));
        }

        public static Vector3 LerpV2(Vector2 a, Vector2 b, Vector2 t)
        {
            return new Vector2(Mathf.Lerp(a.x, b.x, t.x), Mathf.Lerp(a.y, b.y, t.y));
        }

        public static Bounds LerpBounds(Bounds a, Bounds b, float t)
        {
            return new Bounds(Vector3.Lerp(a.center, b.center, t), Vector3.Lerp(a.size, b.size, t));
        }

        public static Bounds LerpBoundsUnconstrained(Bounds a, Bounds b, float t)
        {
            return new Bounds(LerpUnconstrained(a.center, b.center, t), LerpUnconstrained(a.size, b.size, t));
        }

        #endregion Vector Space

        #region Misc

        public static Vector3 Round(this Vector3 v, float resolution = 1.0f)
        {
            float scale = 1.0f / resolution;
            return new Vector3(Mathf.Round(v.x * scale) * resolution, Mathf.Round(v.y * scale) * resolution, Mathf.Round(v.z * scale) * resolution);
        }

        public static int SignWith0(float value)
        {
            return value < 0 ? -1 : (value > 0 ? 1 : 0);
        }

        public static int ModEuclidean(int a, int n)
        {
            return ((a % n) + n) % n;
        }

        public static int CircleDifference(int diff, int range)
        {
            int diff_lo = diff - range;
            int diff_hi = (diff + range) % range;
            if (Mathf.Abs(diff_lo) < Mathf.Abs(diff_hi))
                return diff_lo;
            else
                return diff_hi;
        }

        #endregion Misc
    }
}