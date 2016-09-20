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
using System;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.Animation
{
#if UNITY_EDITOR

    [InitializeOnLoad]
#endif
    [Serializable]
    public class RefBool : IRefBool
    {
        public static readonly GameObject True, False;

        [SerializeField, AutoProperty("Value")]
        private GameObject _ValueObject;

        public bool? Value
        {
            get
            {
                if (_ValueObject == True)
                    return true;
                if (_ValueObject == False)
                    return false;
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    if (value.Value)
                        _ValueObject = True;
                    else
                        _ValueObject = False;
                }
                else
                {
                    _ValueObject = null;
                }
            }
        }

        static RefBool()
        {
            True = Resources.Load<GameObject>("True");
            False = Resources.Load<GameObject>("False");
        }
    }
}