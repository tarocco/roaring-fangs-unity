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
using RoaringFangs.Animation;
using RoaringFangs.ASM;
using RoaringFangs.CameraBehavior;
using RoaringFangs.GSR;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.Editor
{
    public static class MenuItems
    {
        [MenuItem("Roaring Fangs/Animation/Mutex Helper", false, 0)]
        [MenuItem("GameObject/Roaring Fangs/Animation/Mutex Helper", false, 0)]
        [MenuItem("CONTEXT/ControlManager/Mutex Helper", false, 25)]
        public static MutexHelper CreateMutexHelper()
        {
            GameObject selected = Selection.activeGameObject;
            ControlManager manager;
            if (selected != null)
                manager = selected.GetComponentInParent<ControlManager>();
            else
                manager = null;
            if (manager == null)
                throw new Exception("You must select a control for this target group.");

            GameObject mutex_helper_object = new GameObject("New Mutex Helper");
            Undo.RegisterCreatedObjectUndo(mutex_helper_object, "New Mutex Helper");
            MutexHelper mutex_helper = mutex_helper_object.AddComponent<MutexHelper>();

            Undo.SetTransformParent(mutex_helper_object.transform, selected.transform, "New Mutex Helper");
            Selection.activeGameObject = mutex_helper_object;
            return mutex_helper;
        }

        //[MenuItem("Roaring Fangs/GSR/Manual Sorting Order", false, 0)]
        //[MenuItem("GameObject/Roaring Fangs/GSR/Manual Sorting Order", false, 0)]
        [MenuItem("CONTEXT/Renderer/Manual Sorting Order")]
        public static ManualSortingOrder CreateManualSortingOrder()
        {
            var selected_game_object = Selection.activeObject as GameObject;
            if (!selected_game_object)
                throw new InvalidOperationException("No GameObject is selected to add this component to.");
            var self = selected_game_object.AddComponent<ManualSortingOrder>();
            Undo.RegisterCreatedObjectUndo(self, "Add Manual Sorting Order");
            return self;
        }

        //[MenuItem("Roaring Fangs/Camera Behavior/Add Camera Mimick")]
        //[MenuItem("GameObject/Roaring Fangs/Camera Behavior/Add Camera Mimick")]
        [MenuItem("CONTEXT/Camera/Add Camera Mimick")]
        public static CameraMimick CreateCameraMimick()
        {
            var selected_game_object = Selection.activeObject as GameObject;
            if (!selected_game_object)
                throw new InvalidOperationException("No GameObject is selected to add this component to.");
            var self = selected_game_object.AddComponent<CameraMimick>();
            Undo.RegisterCreatedObjectUndo(self, "Add Camera Mimick");
            return self;
        }

        //[MenuItem("Roaring Fangs/GSR/BlitFx Manager")]
        //[MenuItem("GameObject/Roaring Fangs/GSR/BlitFx Manager")]
        [MenuItem("CONTEXT/Camera/Add BlitFx Manager", priority = 1000)]
        public static BlitFxManager CreateBlitFXManager()
        {
            var selected_game_object = Selection.activeObject as GameObject;
            if (!selected_game_object)
                throw new InvalidOperationException("No GameObject is selected to add this component to.");
            var self = selected_game_object.AddComponent<BlitFxManager>();
            Undo.RegisterCreatedObjectUndo(self, "New BlitFx Manager");
            return self;
        }

        //[MenuItem("Roaring Fangs/GSR/BlitFx")]
        //[MenuItem("GameObject/Roaring Fangs/GSR/BlitFx")]
        [MenuItem("CONTEXT/Camera/Add BlitFx", priority = 1010)]
        private static BlitFx CreateBlitFx()
        {
            var selected_game_object = Selection.activeObject as GameObject;
            if (!selected_game_object)
                throw new InvalidOperationException("No GameObject is selected to add this component to.");
            var self = selected_game_object.AddComponent<BlitFx>();
            Undo.RegisterCreatedObjectUndo(self, "New BlitFx");
            return self;
        }

        //[MenuItem("Roaring Fangs/GSR/PreBlitFx")]
        //[MenuItem("GameObject/Roaring Fangs/GSR/PreBlitFx")]
        private static PreBlitFx CreatePreBlitFx()
        {
            var selected_game_object = Selection.activeObject as GameObject;
            if (!selected_game_object)
                throw new InvalidOperationException("No GameObject is selected to add this component to.");
            var self = selected_game_object.AddComponent<PreBlitFx>();
            Undo.RegisterCreatedObjectUndo(self, "New PreBlitFx Texture");
            return self;
        }

        [MenuItem("CONTEXT/BlitFx/Add PreBlitFx")]
        private static PreBlitFx AddPreBlitFx()
        {
            var component = CreatePreBlitFx();
            UnityEditorInternal.ComponentUtility.MoveComponentUp(component);
            return component;
        }
    }
}
