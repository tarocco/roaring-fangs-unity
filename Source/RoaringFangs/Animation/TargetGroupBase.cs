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

#if UNITY_EDITOR

using UnityEditor;

#endif

using RoaringFangs.Utility;
using RoaringFangs.Editor;

namespace RoaringFangs.Animation
{
    [ExecuteInEditMode]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public abstract class TargetGroupBase : MonoBehaviour
    {
        public bool Active
        {
            get { return gameObject.activeInHierarchy; }
            set { gameObject.SetActive(value); }
        }

        public string Name
        {
            get { return gameObject.name; }
            set { gameObject.name = value; }
        }

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            FindIcons();
#endif
        }

#if UNITY_EDITOR
        public static Texture2D HISet { get; protected set; }
        public static Texture2D HIAND { get; protected set; }
        public static Texture2D HIOR { get; protected set; }
        public static Texture2D HIXOR { get; protected set; }

        protected static Texture2D GetIcon(TargetGroupMode mode)
        {
            switch (mode)
            {
                default:
                case TargetGroupMode.Set:
                    return HISet;

                case TargetGroupMode.AND:
                    return HIAND;

                case TargetGroupMode.OR:
                    return HIOR;

                case TargetGroupMode.XOR:
                    return HIXOR;
            }
        }

        public static void FindIcons()
        {
            var icons = HierarchyIcons.GetIcons("TargetGroup", HierarchyIcons.KeyMode.LowerCase);
            HISet = icons.GetOrDefault("mode.set.png");
            HIAND = icons.GetOrDefault("mode.and.png");
            HIOR = icons.GetOrDefault("mode.or.png");
            HIXOR = icons.GetOrDefault("mode.xor.png");
        }

#endif
    }
}