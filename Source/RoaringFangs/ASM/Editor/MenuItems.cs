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
using RoaringFangs.ASM;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.Editor
{
    public static class MenuItems
    {
        [MenuItem("Roaring Fangs/ASM/Controlled State Manager")]
        [MenuItem("GameObject/Roaring Fangs/ASM/Controlled State Manager")]
        //[UnityEditor.MenuItem("CONTEXT/Animator/Controlled State Manager")]
        public static ControlledStateManager CreateControlledStateManager()
        {
            var created_game_object = new GameObject("New Controlled State Manager");
            var selected_game_object = Selection.activeObject as GameObject;
            if (selected_game_object)
                created_game_object.transform.SetParent(selected_game_object.transform);
            var configurator_cache = new GameObject("Configuration Object Cache");
            configurator_cache.transform.SetParent(created_game_object.transform);
            var self = created_game_object.AddComponent<ControlledStateManager>();
            self.ConfigurationObjectCache = configurator_cache.transform;
            Selection.activeObject = created_game_object;
            Undo.RegisterCreatedObjectUndo(created_game_object, "New Controlled State Manager");
            return self;
        }
    }
}
