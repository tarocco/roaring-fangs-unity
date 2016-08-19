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
    public class StatefulSceneLoader : MonoBehaviour
    {
        private class SceneUnloadCoroutineProcessor : MonoBehaviour { }

        public SceneLoadCompleteEvent SceneLoadComplete;

        [SerializeField]
        private string _SceneName;
        public string SceneName
        {
            get { return _SceneName; }
            protected set { _SceneName = value; }
        }

        public Scenes.LoadMode Mode = Scenes.LoadMode.CheckAdd;

        [Header("Only Async mode permitted")]
        [SerializeField, ReadOnly]
        private readonly bool _Async = true;

        private GameObject _CoroutineObject;
        private Scenes.CoroutineProcessor _CoroutineProcessor;

        public bool LoadOnStart = false;
        public bool UnloadOnDestroy = false;

        public void Load()
        {
            Scenes.StartLoadAsync(_CoroutineProcessor, SceneName, Mode, SceneLoadComplete.Invoke);
        }

        public void Unload()
        {
            // TODO: callback
            if(_CoroutineObject != null)
                Scenes.StartUnloadAsync(_CoroutineProcessor, SceneName, (s, a) => Destroy(_CoroutineObject));
        }

        private void Awake()
        {
            _CoroutineObject = new GameObject("Scene Unloader");
            _CoroutineObject.hideFlags = HideFlags.HideInHierarchy;
            _CoroutineProcessor = _CoroutineObject.AddComponent<Scenes.CoroutineProcessor>();
            DontDestroyOnLoad(_CoroutineObject);
        }

        private void Start()
        {
            if (LoadOnStart)
                Load();
        }

        public void OnDestroy()
        {
            if (UnloadOnDestroy)
                Unload();
            else
                Destroy(_CoroutineObject);
        }

        public void OnLoadThisSceneNext(object sender, SceneLoadCompleteEventArgs args)
        {
            Load();
        }
    }
}