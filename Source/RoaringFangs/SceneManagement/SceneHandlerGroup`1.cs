using RoaringFangs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace RoaringFangs.SceneManagement
{
    [Serializable]
    public abstract class SceneHandlerGroup<TSceneHandler> :
        ISceneHandlerGroup<TSceneHandler>
        where TSceneHandler : ISceneHandler
    {
        [SerializeField]
        private SceneLoadManyCompleteEvent _LoadComplete =
            new SceneLoadManyCompleteEvent();

        public SceneLoadManyCompleteEvent LoadComplete
        {
            get { return _LoadComplete; }
            protected set { _LoadComplete = value; }
        }

        [SerializeField]
        private SceneUnloadManyCompleteEvent _UnloadComplete =
            new SceneUnloadManyCompleteEvent();

        public SceneUnloadManyCompleteEvent UnloadComplete
        {
            get { return _UnloadComplete; }
            protected set { _UnloadComplete = value; }
        }

        public abstract IEnumerable<TSceneHandler> SceneHandlers
        {
            get;
            protected set;
        }

        // NonSerializedAttribute is necessary to make private fields work "normally" in ScriptableObjects
        // See this post for more info about ScriptableObject shenanigans:
        // http://forum.unity3d.com/threads/scriptableobject-is-it-supposed-to-save-its-state-or-isnt-it.80777/

        [NonSerialized]
        private List<string>
            _LoadChecklist = new List<string>(),
            _UnloadChecklist = new List<string>();

        [NonSerialized]
        protected List<SceneLoadCompleteEventArgs> _LoadCollectedEventArgs =
            new List<SceneLoadCompleteEventArgs>();

        [NonSerialized]
        protected List<SceneUnloadCompleteEventArgs> _UnloadCollectedEventArgs =
            new List<SceneUnloadCompleteEventArgs>();

        private void HandleLoadComplete(object sender, SceneLoadCompleteEventArgs args)
        {
            var loaded_scene = args.LoadedScene;
            var loaded_scene_name = loaded_scene.name;
            _LoadChecklist.Remove(loaded_scene_name);
            if (_LoadChecklist.Count == 0)
                OnLoadChecklistComplete();

            var handler = (SceneHandler)sender;
            handler.LoadComplete.RemoveListener(HandleLoadComplete);
        }

        private void HandleUnloadComplete(object sender, SceneUnloadCompleteEventArgs args)
        {
            var unloaded_scene_name = args.UnloadedSceneName;
            _UnloadChecklist.Remove(unloaded_scene_name);
            if (_LoadChecklist.Count == 0)
                OnUnloadChecklistComplete();

            var handler = (SceneHandler)sender;
            handler.UnloadComplete.RemoveListener(HandleUnloadComplete);
        }

        protected virtual void OnLoadChecklistComplete()
        {
            var loaded_args_array = _LoadCollectedEventArgs.ToArray();
            var loaded_args = new SceneLoadManyCompleteEventArgs(loaded_args_array);
            LoadComplete.Invoke(this, loaded_args);
        }

        protected virtual void OnUnloadChecklistComplete()
        {
            var unloaded_args_array = _UnloadCollectedEventArgs.ToArray();
            var unloaded_args = new SceneUnloadManyCompleteEventArgs(unloaded_args_array);
            UnloadComplete.Invoke(this, unloaded_args);
        }

        public void StartLoadAsync(MonoBehaviour self)
        {
            {
                bool ready = _LoadChecklist == null || _LoadChecklist.Count == 0;
                Debug.Assert(ready, "StartLoadAsync called while scenes are still being loaded!");
            }
            var scene_names = SceneHandlers.Select(h => h.SceneName);
            _LoadChecklist = new List<string>(scene_names);
            _LoadCollectedEventArgs = new List<SceneLoadCompleteEventArgs>();
            foreach (var handler in SceneHandlers)
            {
                handler.LoadComplete.AddListener(HandleLoadComplete);
                handler.StartLoadAsync(self);
            }
        }

        public void StartUnloadAsync(MonoBehaviour self)
        {
            {
                bool ready = _UnloadChecklist == null || _UnloadChecklist.Count == 0;
                Debug.Assert(ready, "StartUnloadAsync called while scenes are still being unloaded!");
            }
            var scene_names = SceneHandlers.Select(h => h.SceneName);
            _UnloadChecklist = new List<string>(scene_names);
            _UnloadCollectedEventArgs = new List<SceneUnloadCompleteEventArgs>();
            foreach (var handler in SceneHandlers)
            {
                handler.UnloadComplete.AddListener(HandleUnloadComplete);
                handler.StartUnloadAsync(self);
            }
        }

        public SceneHandlerGroup(params TSceneHandler[] scene_handlers)
        {
            SceneHandlers = new List<TSceneHandler>(scene_handlers);
        }
    }
}