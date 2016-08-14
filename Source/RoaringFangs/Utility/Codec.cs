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
    public static partial class Codec
    {
        #region Color

        public const float RecipByteMaxF = 0.0039215686274509803921568627451f;
        public const float ByteMaxF = 255f;

        public static Color ARGB32ToColor(uint argb)
        {
            return new Color(
                RecipByteMaxF * ((argb >> 16) & 0xFF),
                RecipByteMaxF * ((argb >> 8) & 0xFF),
                RecipByteMaxF * (argb & 0xFF),
                RecipByteMaxF * (argb >> 24));
        }

        public static Color HexStringToColor(string argb_string)
        {
            if (argb_string.Length == 6)
                argb_string = "FF" + argb_string;
            uint argb = uint.Parse(argb_string, System.Globalization.NumberStyles.HexNumber);
            return ARGB32ToColor(argb);
        }

        public static int ColorToARGB32(Color color)
        {
            return (Mathf.FloorToInt(ByteMaxF * color.a) << 24)
                | (Mathf.FloorToInt(ByteMaxF * color.r) << 16)
                | (Mathf.FloorToInt(ByteMaxF * color.g) << 8)
                | Mathf.FloorToInt(ByteMaxF * color.b);
        }

        public static string ColorToHexString(Color color)
        {
            return ColorToARGB32(color).ToString("X");
        }

        #endregion Color

        #region Time

        public static string SecondsToMinSecMS(float seconds, bool always_show_minutes = true)
        {
            int seconds_floored_abs = Mathf.Abs(Mathf.FloorToInt(seconds));
            int ms = Mathf.Abs(Mathf.FloorToInt(seconds * 1000)) % 1000;
            int sec = seconds_floored_abs % 60;
            int min = seconds_floored_abs / 60;
            string format = always_show_minutes || min > 0 ? "{0}{1}:{2,2:D2}:{3,3:D3}" : "{0}{2,2:D2}:{3,3:D3}";
            return String.Format(format, seconds < 0 ? "-" : "", min, sec, ms);
        }

        #endregion Time
    }
}