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

        public enum LoadMode
        {
            Add,
            CheckAdd,
            UnloadAdd,
        }
        public string SceneName;
        public LoadMode Mode = LoadMode.CheckAdd;
        public bool Async = true;
        void Start()
        {
            if (Async)
                StartCoroutine(LoadAsync(SceneName, Mode).GetEnumerator());
            else
                Load(SceneName, Mode);
        }

        protected void Load(string scene_name, LoadMode mode)
        {
            switch (mode)
            {
                default:
                case LoadMode.Add:
                    SceneManager.LoadScene(scene_name, LoadSceneMode.Additive);
                    break;
                case LoadMode.CheckAdd:
                    if (!SceneManager.GetSceneByName(scene_name).isLoaded)
                        SceneManager.LoadScene(scene_name, LoadSceneMode.Additive);
                    // else throw an exception (should it?)
                    break;
                case LoadMode.UnloadAdd:
                    SceneManager.UnloadScene(scene_name);
                    SceneManager.LoadScene(scene_name, LoadSceneMode.Additive);
                    break;
            }
            Scene loaded = SceneManager.GetSceneByName(scene_name);
            SceneLoadComplete.Invoke(this, new SceneLoadCompleteEventArgs(loaded));
        }

        protected IEnumerable LoadAsync(string scene_name, LoadMode mode)
        {
            // This is here because of a Mono bug...
            YieldInstruction operation = null;
            switch (mode)
            {
                default:
                case LoadMode.Add:
                    operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                    break;
                case LoadMode.CheckAdd:
                    if (!SceneManager.GetSceneByName(scene_name).isLoaded)
                        operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                    // TODO: else throw an exception (should it?)
                    break;
                case LoadMode.UnloadAdd:
                    SceneManager.UnloadScene(scene_name);
                    operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                    break;
            }
            yield return operation;
            Scene loaded = SceneManager.GetSceneByName(scene_name);
            SceneLoadComplete.Invoke(this, new SceneLoadCompleteEventArgs(loaded));
        }
    }
}


