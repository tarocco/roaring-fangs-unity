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