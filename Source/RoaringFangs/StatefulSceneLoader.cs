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
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace RoaringFangs
{
    public class StatefulSceneLoader : SceneLoader
    {
        [Serializable]
        public class SceneLoadCompleteEvent : UnityEvent<object, Scenes.SceneLoadCompleteEventArgs> { }

        public SceneLoadCompleteEvent SceneLoadComplete = new SceneLoadCompleteEvent();

        public string SceneName;
        public Scenes.LoadMode Mode = Scenes.LoadMode.CheckAdd;
        public bool Async = true;
        public bool LoadAtStart = false;

        private void Start()
        {
            if (LoadAtStart)
                Load();
        }

        public void Load()
        {
            Load(SceneName, Mode, Async, SceneLoadComplete.Invoke);
        }

        public void OnLoadThisSceneNext(object sender, Scenes.SceneLoadCompleteEventArgs args)
        {
            Load();
        }
    }
}