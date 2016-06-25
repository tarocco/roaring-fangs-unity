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
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using System;
using System.Collections;

namespace RoaringFangs.Utility
{
    public class SceneLoader : MonoBehaviour
    {
        #region Events
        public class SceneLoadCompleteEventArgs : EventArgs
        {
            public readonly Scene Scene;
            public SceneLoadCompleteEventArgs(Scene scene)
            {
                Scene = scene;
            }
        }
        public delegate void SceneLoadCompletedHandler(object sender, SceneLoadCompleteEventArgs args);
        [Serializable]
        public class SceneLoadCompleteEvent : UnityEvent<object, SceneLoadCompleteEventArgs> { }
        #endregion

        public SceneLoadCompleteEvent SceneLoadComplete = new SceneLoadCompleteEvent();

        public string SceneName;
        public Scenes.LoadMode Mode = Scenes.LoadMode.CheckAdd;
        public bool Async = true;
        public bool LoadAtStart = true;

        void Start()
        {
            if (LoadAtStart)
                DoLoad();
        }

        private void DoLoad()
        {
            if (Async)
                StartCoroutine(DoLoadAsync().GetEnumerator());
            else
            {
                Scenes.Load(SceneName, Mode);
                Scene loaded = SceneManager.GetSceneByName(SceneName);
                SceneLoadComplete.Invoke(this, new SceneLoadCompleteEventArgs(loaded));
            }
        }

        private IEnumerable DoLoadAsync()
        {
            var operations = Scenes.LoadAsync(SceneName, Mode);
            foreach (var y in operations)
                yield return y;
            Scene loaded = SceneManager.GetSceneByName(SceneName);
            SceneLoadComplete.Invoke(this, new SceneLoadCompleteEventArgs(loaded));
        }

        public void OnLoadThisSceneNext(object sender, SceneLoadCompleteEventArgs args)
        {
            DoLoad();
        }
    }
}