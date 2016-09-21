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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoaringFangs.SceneManagement
{
    [Serializable]
    public class SceneHandlerGroup : ISceneHandlerGroup<ISceneHandler>
    {
        [SerializeField]
        private SceneLoadManyCompleteEvent _LoadComplete =
            new SceneLoadManyCompleteEvent();

        public SceneLoadManyCompleteEvent LoadComplete
        {
            get { return _LoadComplete; }
            protected set { _LoadComplete = value; }
        }

        [SerializeField]
        private SceneUnloadManyCompleteEvent _UnloadComplete =
            new SceneUnloadManyCompleteEvent();

        public SceneUnloadManyCompleteEvent UnloadComplete
        {
            get { return _UnloadComplete; }
            protected set { _UnloadComplete = value; }
        }

        [SerializeField]
        private List<ISceneHandler> _SceneHandlers;

        public IEnumerable<ISceneHandler> SceneHandlers
        {
            get
            {
                return _SceneHandlers;
            }
            protected set
            {
                _SceneHandlers = value.ToList();
            }
        }

        private List<string>
            _LoadChecklist = new List<string>(),
            _UnloadChecklist = new List<string>();

        protected List<SceneLoadCompleteEventArgs> _LoadCollectedEventArgs =
            new List<SceneLoadCompleteEventArgs>();

        protected List<SceneUnloadCompleteEventArgs> _UnloadCollectedEventArgs =
            new List<SceneUnloadCompleteEventArgs>();

        private void HandleLoadComplete(object sender, SceneLoadCompleteEventArgs args)
        {
            var loaded_scene = args.LoadedScene;
            var loaded_scene_name = loaded_scene.name;
            _LoadChecklist.Remove(loaded_scene_name);
            if (_LoadChecklist.Count == 0)
                OnLoadChecklistComplete();

            var handler = (SceneHandler)sender;
            handler.LoadComplete.RemoveListener(HandleLoadComplete);
        }

        private void HandleUnloadComplete(object sender, SceneUnloadCompleteEventArgs args)
        {
            var unloaded_scene_name = args.UnloadedSceneName;
            _UnloadChecklist.Remove(unloaded_scene_name);
            if (_LoadChecklist.Count == 0)
                OnUnloadChecklistComplete();

            var handler = (SceneHandler)sender;
            handler.UnloadComplete.RemoveListener(HandleUnloadComplete);
        }

        protected virtual void OnLoadChecklistComplete()
        {
            var loaded_args_array = _LoadCollectedEventArgs.ToArray();
            var loaded_args = new SceneLoadManyCompleteEventArgs(loaded_args_array);
            LoadComplete.Invoke(this, loaded_args);
        }

        protected virtual void OnUnloadChecklistComplete()
        {
            var unloaded_args_array = _UnloadCollectedEventArgs.ToArray();
            var unloaded_args = new SceneUnloadManyCompleteEventArgs(unloaded_args_array);
            UnloadComplete.Invoke(this, unloaded_args);
        }

        public void StartLoadAsync(MonoBehaviour self)
        {
            {
                bool ready = _LoadChecklist == null || _LoadChecklist.Count == 0;
                Debug.Assert(ready, "StartLoadAsync called while scenes are still being loaded");
            }
            var scene_names = SceneHandlers.Select(h => h.SceneName);
            _LoadChecklist = new List<string>(scene_names);
            _LoadCollectedEventArgs = new List<SceneLoadCompleteEventArgs>();
            foreach (var handler in SceneHandlers)
            {
                handler.LoadComplete.AddListener(HandleLoadComplete);
                handler.StartLoadAsync(self);
            }
        }

        public void StartUnloadAsync(MonoBehaviour self)
        {
            {
                bool ready = _UnloadChecklist == null || _UnloadChecklist.Count == 0;
                Debug.Assert(ready, "StartUnloadAsync called while scenes are still being unloaded!");
            }
            var scene_names = SceneHandlers.Select(h => h.SceneName);
            _UnloadChecklist = new List<string>(scene_names);
            _UnloadCollectedEventArgs = new List<SceneUnloadCompleteEventArgs>();
            foreach (var handler in SceneHandlers)
            {
                handler.UnloadComplete.AddListener(HandleUnloadComplete);
                handler.StartUnloadAsync(self);
            }
        }

        public void AddSceneHandlers(params ISceneHandler[] handlers)
        {
            _SceneHandlers.AddRange(handlers);
        }

        public void RemoveSceneHandler(ISceneHandler handler)
        {
            _SceneHandlers.Remove(handler);
        }

        public SceneHandlerGroup(params ISceneHandler[] scene_handlers)
        {
            SceneHandlers = new List<ISceneHandler>(scene_handlers);
        }
    }
}