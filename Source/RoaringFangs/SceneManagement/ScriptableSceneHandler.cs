using RoaringFangs;
using System;
using UnityEngine;

namespace RoaringFangs.SceneManagement
{
    [CreateAssetMenu(
        fileName = "New Scene Handler",
        menuName = "Roaring Fangs/Scene Managment/Scene Handler")]
    public class ScriptableSceneHandler : ScriptableObject, ISceneHandler
    {
        [SerializeField]
        private SceneHandler _Self;

        public SceneLoadCompleteEvent LoadComplete
        {
            get
            {
                return _Self.LoadComplete;
            }
        }

        public string SceneName
        {
            get
            {
                return _Self.SceneName;
            }
        }

        public SceneUnloadCompleteEvent UnloadComplete
        {
            get
            {
                return _Self.UnloadComplete;
            }
        }

        public void StartLoadAsync(MonoBehaviour self)
        {
            _Self.StartLoadAsync(self);
        }

        public void StartUnloadAsync(MonoBehaviour self)
        {
            _Self.StartUnloadAsync(self);
        }
    }
}