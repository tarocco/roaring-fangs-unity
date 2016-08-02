using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RoaringFangs
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

        protected void PushScene(string scene_name)
        {
            _SceneStack.Push(scene_name);
        }

        protected string PopScene()
        {
            if (_SceneStack.Count == 1)
                throw new InvalidOperationException("Cannot remove base of navigation stack.");
            return _SceneStack.Pop();
        }

        public void ResetToActiveScene()
        {
            _SceneStack.Clear();
            _SceneStack.Push(SceneManager.GetActiveScene().name);
        }

        protected void HandleLoadedNext(object sender, SceneLoadCompleteEventArgs args)
        {
            SceneManager.SetActiveScene(args.LoadedScene);
            PushScene(args.LoadedScene.name);
        }

        public virtual void NavigateScene(string name)
        {
            Scenes.Load(this, name, Scenes.LoadMode.ReplaceActive, true, HandleLoadedNext);
        }

        protected void HandleLoadedAndReset(object sender, SceneLoadCompleteEventArgs args)
        {
            SceneManager.SetActiveScene(args.LoadedScene);
            ResetToActiveScene();
        }

        public virtual void ResetToScene(string name)
        {
            Scenes.Load(this, name, Scenes.LoadMode.ReplaceActive, true, HandleLoadedAndReset);
        }

        protected void HandleLoadedPrevious(object sender, SceneLoadCompleteEventArgs args)
        {
            SceneManager.SetActiveScene(args.LoadedScene);
            PopScene();
        }

        public virtual void NavigatePrevious()
        {
            try
            {
                Scenes.Load(this, PreviousSceneName, Scenes.LoadMode.ReplaceActive, true, HandleLoadedPrevious);
            }
            catch(InvalidOperationException ex)
            {
                Debug.LogException(ex);
            }
        }

        void Start()
        {
            ResetToActiveScene();
        }

#if UNITY_EDITOR
        private static IEnumerable<string> PadFront(IEnumerable<string> strings, int cap)
        {
            for (int i = (int)strings.Count(); i < cap; i++)
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