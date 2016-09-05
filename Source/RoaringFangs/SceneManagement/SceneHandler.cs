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