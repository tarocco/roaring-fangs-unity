using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RoaringFangs.Utility;

namespace RoaringFangs.SceneManagement
{
    public class Navigation : MonoBehaviour
    {
        private Stack<string> _SceneStack =
            new Stack<string>();

        public IEnumerable<string> NavigatedSceneNames
        {
            get
            {
                return _SceneStack;
            }
        }

        public string CurrentSceneName
        {
            get
            {
                return _SceneStack.Peek();
            }
        }

        public string PreviousSceneName
        {
            get
            {
                if (_SceneStack.Count == 1)
                    throw new InvalidOperationException("No previous scene available in the navigation stack.");
                return _SceneStack.Skip(1).First();
            }
        }

        public void ResetToActiveScene()
        {
            _SceneStack.Clear();
            _SceneStack.Push(SceneManager.GetActiveScene().name);
        }

        private void HandleCurrentSceneUnloaded(object sender, SceneUnloadCompleteEventArgs args)
        {
            _NextSceneHandler.StartLoadAsync(this);
        }

        private void HandlePushSceneLoadComplete(object sender, SceneLoadCompleteEventArgs args)
        {
            _SceneStack.Push(args.LoadedScene.name);
        }

        private void HandlePopSceneLoadComplete(object sender, SceneLoadCompleteEventArgs args)
        {
            _SceneStack.Pop();
        }

        private SceneHandler _CurrentSceneHandler;
        private SceneHandler _NextSceneHandler;

        public virtual void PushScene(string name)
        {
            _CurrentSceneHandler = new SceneHandler(CurrentSceneName, Scenes.LoadMode.SetActive);
            _NextSceneHandler = new SceneHandler(name, Scenes.LoadMode.SetActive);
            _CurrentSceneHandler.UnloadComplete.AddListener(HandleCurrentSceneUnloaded);
            _NextSceneHandler.LoadComplete.AddListener(HandlePushSceneLoadComplete);
            _CurrentSceneHandler.StartUnloadAsync(this);
        }

        public virtual void PopScene()
        {
            if (_SceneStack.Count <= 1)
                throw new InvalidOperationException("Cannot remove base of navigation stack.");
            string name = PreviousSceneName;
            _CurrentSceneHandler = new SceneHandler(CurrentSceneName, Scenes.LoadMode.SetActive);
            _NextSceneHandler = new SceneHandler(name, Scenes.LoadMode.SetActive);
            _CurrentSceneHandler.UnloadComplete.AddListener(HandleCurrentSceneUnloaded);
            _NextSceneHandler.LoadComplete.AddListener(HandlePopSceneLoadComplete);
            _CurrentSceneHandler.StartUnloadAsync(this);
        }

        void Start()
        {
            //ResetToActiveScene();
        }

#if UNITY_EDITOR
        private static IEnumerable<string> PadFront(IEnumerable<string> strings, int cap)
        {
            for (int i = (int)strings.LongCount(); i < cap; i++)
                yield return "";
            foreach (string s in strings)
                yield return s;
        }

        void OnGUI()
        {
            var scenes = String.Join("\n", NavigatedSceneNames.Reverse().ToArray());
            UnityEngine.GUI.Label(new Rect(Vector2.zero, new Vector2(256f, 256f)), scenes);
        }
#endif
    }
}