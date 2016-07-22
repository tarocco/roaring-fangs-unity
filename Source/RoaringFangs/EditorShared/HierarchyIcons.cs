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

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.Editor
{
    [InitializeOnLoad]
    internal static class HierarchyIcons
    {
        public enum KeyMode
        {
            Normal,
            LowerCase,
            UpperCase
        }

        public static IDictionary<string, Texture2D> GetIcons(string class_name, KeyMode key_mode = KeyMode.Normal)
        {
            string label = class_name + ".HierarchyIcon";
            var asset_guids = AssetDatabase.FindAssets("t:texture2D l:" + label);
            var asset_paths = asset_guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid));
            var key_function = GetKeyFunction(key_mode);
            return asset_paths.ToDictionary(
                path => key_function(Path.GetFileName(path)),
                path => AssetDatabase.LoadAssetAtPath<Texture2D>(path));
        }

        private static Func<string, string> GetKeyFunction(KeyMode key_mode)
        {
            switch (key_mode)
            {
                default:
                case KeyMode.Normal:
                    return Plain;

                case KeyMode.LowerCase:
                    return ToLower;

                case KeyMode.UpperCase:
                    return ToUpper;
            }
        }

        private static string Plain(string str)
        {
            return str;
        }

        private static string ToLower(string str)
        {
            return str.ToLower();
        }

        private static string ToUpper(string str)
        {
            return str.ToUpper();
        }

        private static void HandleHierarchyWindowItemOnGUI(int instance_id, Rect selection)
        {
            var @object = EditorUtility.InstanceIDToObject(instance_id);
            if (@object is GameObject)
            {
                var game_object = @object as GameObject;
                var icon_haver = game_object.GetComponent<IHasHierarchyIcons>();
                if (icon_haver != null)
                {
                    Rect icon_position = new Rect(selection);
                    icon_position.width = 20f;
                    icon_position.height = 20f;
                    icon_position.x = selection.xMax - icon_position.width;
                    icon_haver.OnDrawHierarchyIcons(icon_position);
                }
            }
        }

        static HierarchyIcons()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
        }
    }
}

#endif