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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace RoaringFangs.Utility
{
    public class SceneLoadChecklist : MonoBehaviour
    {
        [Serializable]
        public class ChecklistEntry
        {
            public string Name;
            public bool Check;
            public SceneLoadCompleteEventArgs SceneLoadArgs;
        }

        public class ChecklistCompleteEventArgs : EventArgs
        {
            public IEnumerable<SceneLoadCompleteEventArgs> SceneLoadArgs;
            public ChecklistCompleteEventArgs(
                IEnumerable<SceneLoadCompleteEventArgs> scene_load_args)
            {
                SceneLoadArgs = scene_load_args;
            }
        }

        [Serializable]
        public class ChecklistCompleteEvent :
            UnityEvent<object, ChecklistCompleteEventArgs>
        { }

        public ChecklistCompleteEvent SceneLoadComplete;

        [SerializeField, AutoProperty]
        private List<ChecklistEntry> _Checklist;
        public IEnumerable<ChecklistEntry> Checklist
        {
            get { return _Checklist; }
            protected set { _Checklist = value.ToList(); }
        }

        public void OnSceneLoad(
            object sender,
            SceneLoadCompleteEventArgs args)
        {
            string loaded_scene_name = args.LoadedScene.name;
            var entries_to_check = Checklist
                .Where(c => c.Name == args.LoadedScene.name);
            foreach (var entry in entries_to_check)
            {
                entry.Check = true;
                entry.SceneLoadArgs = args;
            }
            bool all_checked = Checklist.All(c => c.Check);
            if (all_checked)
            {
                var all_scene_args = Checklist
                    .Select(c => c.SceneLoadArgs).ToArray();
                var checklist_args = new ChecklistCompleteEventArgs(all_scene_args);
                SceneLoadComplete.Invoke(this, checklist_args);
            }
        }
    }
}