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

namespace RoaringFangs.SceneManagement
{
    [Serializable]
    public class SceneHandler : ISceneHandler
    {
        [SerializeField]
        private string _SceneName;

        public string SceneName
        {
            get { return _SceneName; }
            set { _SceneName = value; }
        }

        [SerializeField]
        private Scenes.LoadMode _LoadMode;

        public Scenes.LoadMode LoadMode
        {
            get { return _LoadMode; }
            set { _LoadMode = value; }
        }

        [SerializeField]
        private SceneLoadCompleteEvent _LoadComplete =
            new SceneLoadCompleteEvent();

        public SceneLoadCompleteEvent LoadComplete
        {
            get { return _LoadComplete; }
            protected set { _LoadComplete = value; }
        }

        [SerializeField]
        private SceneUnloadCompleteEvent _UnloadComplete =
            new SceneUnloadCompleteEvent();

        public SceneUnloadCompleteEvent UnloadComplete
        {
            get { return _UnloadComplete; }
            protected set { _UnloadComplete = value; }
        }

        public void StartLoadAsync(MonoBehaviour self)
        {
            StartLoadAsync(self, SceneName, LoadMode);
        }

        public void StartUnloadAsync(MonoBehaviour self)
        {
            StartUnloadAsync(self, SceneName);
        }

        private void HandleLoadComplete(object sender, SceneLoadCompleteEventArgs args)
        {
            LoadComplete.Invoke(this, args);
        }

        private void HandleUnloadComplete(object sender, SceneUnloadCompleteEventArgs args)
        {
            UnloadComplete.Invoke(this, args);
        }

        private void StartLoadAsync(MonoBehaviour self, string scene_name, Scenes.LoadMode mode)
        {
            Scenes.StartLoadAsync(self, scene_name, mode, HandleLoadComplete);
        }

        private void StartUnloadAsync(MonoBehaviour self, string scene_name)
        {
            Scenes.StartUnloadAsync(self, scene_name, HandleUnloadComplete);
        }

        public SceneHandler(string scene_name, Scenes.LoadMode load_mode)
        {
            SceneName = scene_name;
            LoadMode = load_mode;
        }
    }
}