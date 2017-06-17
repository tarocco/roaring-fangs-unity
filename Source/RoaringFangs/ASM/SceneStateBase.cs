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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace RoaringFangs.ASM
{
    public abstract class SceneStateBase : StateController
    {
        public enum SceneLoadDirectiveType
        {
            Name,
            Parameter
        }

        [Serializable]
        public struct SceneLoadDirective
        {
#if ODIN_INSPECTOR

            [HorizontalGroup]
#endif
            public string Value;

#if ODIN_INSPECTOR

            [HorizontalGroup]
#endif
            public SceneLoadDirectiveType Type;
        }

        [SerializeField]
        private string _Tag;

        public override string Tag
        {
            get { return _Tag; }
            set { _Tag = value; }
        }

#if ODIN_INSPECTOR
        [HideLabel]
        [Header("Active Scene")]
#endif
        public SceneLoadDirective ActiveScene;

        /// <summary>
        /// The name of the scene to be set active after entering this state
        /// </summary>
        public string ActiveSceneName
        {
            get { return ResolveSceneName(ActiveScene); }
        }

        [SerializeField]
        private SceneLoadDirective[] _FirstScenesEntryLoad;

        [SerializeField]
        private SceneLoadDirective[] _SecondScenesEntryLoad;

        [SerializeField]
        private SceneLoadDirective[] _FirstScenesExitUnload;

        [SerializeField]
        private SceneLoadDirective[] _SecondScenesExitUnload;

        private string ResolveSceneName(string value, SceneLoadDirectiveType type)
        {
            switch (type)
            {
                default:
                case SceneLoadDirectiveType.Name:
                    return value;

                case SceneLoadDirectiveType.Parameter:
                    return CachedControlledStateManager.ParameterEntriesLookup[value].First();
            }
        }

        private string ResolveSceneName(SceneLoadDirective directive)
        {
            return ResolveSceneName(directive.Value, directive.Type);
        }

        public IEnumerable<string> FirstScenesEntryLoad
        {
            get
            {
                return _FirstScenesEntryLoad
                  .Select(ResolveSceneName)
                  .ToArray();
            }
        }

        public IEnumerable<string> SecondScenesEntryLoad
        {
            get
            {
                return _SecondScenesEntryLoad
                  .Select(ResolveSceneName)
                  .ToArray();
            }
        }

        public IEnumerable<string> FirstScenesExitUnload
        {
            get
            {
                return _FirstScenesExitUnload
                  .Select(ResolveSceneName)
                  .ToArray();
            }
        }

        public IEnumerable<string> SecondScenesExitUnload
        {
            get
            {
                return _SecondScenesExitUnload
                  .Select(ResolveSceneName)
                  .ToArray();
            }
        }

        public List<GameObject> ConfigurationObjects;

        private IEnumerable OnStateEnterCoroutine(IEnumerable<string> scene_names)
        {
            foreach (object o in Scenes.LoadTogether(FirstScenesEntryLoad))
                yield return o;
            foreach (object o in Scenes.LoadTogether(SecondScenesEntryLoad))
                yield return o;
            string active_scene_name = ActiveSceneName;
            if (String.IsNullOrEmpty(active_scene_name))
            {
                Debug.LogWarning("No active scene name specified");
            }
            else
            {
                Scene active_scene = SceneManager.GetSceneByName(active_scene_name);
                if (!active_scene.IsValid())
                    Debug.LogWarning("Specified active scene is invalid\nScene name: \"" + active_scene_name + "\"");
                else if (!active_scene.isLoaded)
                    Debug.LogWarning("Specified active scene is valid but not yet loaded\nScene name:\"" + active_scene_name + "\"");
                else
                    SceneManager.SetActiveScene(active_scene);
            }
            foreach (var config_object in ConfigurationObjects)
                Instantiate(config_object);
        }

        private IEnumerable OnStateExitCoroutine(IEnumerable<string> scene_names)
        {
            foreach (object o in Scenes.UnloadTogether(scene_names))
                yield return o;
        }

        protected void DoVerifyEnter()
        {
            var scene_names_to_load = FirstScenesEntryLoad
                .Concat(SecondScenesEntryLoad)
                .ToArray();
            foreach (var scene_name in scene_names_to_load)
            {
                // Verify that the scenes to be loaded are valid before enqueing the loader coroutine
                if (!Application.CanStreamedLevelBeLoaded(scene_name))
                    throw new ArgumentException("Scene cannot not be loaded: \"" + scene_name + "\"");
                // Verify that none of the scenes to be loaded are already loaded
                // This limitation is caused by SceneManager.SetActiveScene being really BAD
                var scene = SceneManager.GetSceneByName(scene_name);
                if (scene.isLoaded)
                    throw new InvalidOperationException("Scene cannot be loaded more than once: \"" + scene_name + "\"");
            }
        }

        protected void DoVerifyExit()
        {
            var scene_names_to_unload = FirstScenesExitUnload
                .Concat(SecondScenesExitUnload)
                .ToArray();
            // Verify that the scenes to be unloaded are valid before enqueing the unloader coroutine
            foreach (var scene_name in scene_names_to_unload)
                if (!Application.CanStreamedLevelBeLoaded(scene_name))
                    throw new ArgumentException("Scene cannot not be unloaded: \"" + scene_name + "\"");
        }

        protected void DoEnter(ControlledStateManager manager)
        {
            var scene_names_to_load = FirstScenesEntryLoad
                .Concat(SecondScenesEntryLoad)
                .ToArray();
            //manager.EnqueueDeferredCoroutine(OnStateEnterCoroutine(scene_names_to_load).GetEnumerator());
            manager.EnqueueCoroutine(OnStateEnterCoroutine(scene_names_to_load).GetEnumerator());
        }

        protected void DoExit(ControlledStateManager manager)
        {
            var scene_names_to_unload = FirstScenesExitUnload
                .Concat(SecondScenesExitUnload)
                .ToArray();
            manager.EnqueueCoroutine(OnStateExitCoroutine(scene_names_to_unload).GetEnumerator());
        }

        public override void Initialize(ControlledStateManager manager)
        {
            base.Initialize(manager);
            // Replace configuration objects with instances (don't work directly on prefabs!)
            var instances = new List<GameObject>();
            foreach (var configuration_object in ConfigurationObjects)
            {
                var instance = Instantiate(configuration_object, manager.ConfigurationObjectCache);
                instance.name = configuration_object.name; // Remove "(Clone)" suffix
                instances.Add(instance);
            }
            ConfigurationObjects = instances;
        }

        public override void OnManagedStateVerifyEnter(
            ControlledStateManager manager,
            ManagedStateEventArgs args)
        {
            DoVerifyEnter();
        }

        public override void OnManagedStateEnter(
            ControlledStateManager manager,
            ManagedStateEventArgs args)
        {
            DoEnter(manager);
        }

        public override void OnManagedStateVerifyExit(
            ControlledStateManager manager,
            ManagedStateEventArgs args)
        {
            DoVerifyExit();
        }

        public override void OnManagedStateExit(
            ControlledStateManager manager,
            ManagedStateEventArgs args)
        {
            DoExit(manager);
        }

        public override void OnManagedStateMachineVerifyEnter(
            ControlledStateManager manager,
            ManagedStateMachineEventArgs args)
        {
            DoVerifyEnter();
        }

        public override void OnManagedStateMachineEnter(
            ControlledStateManager manager,
            ManagedStateMachineEventArgs args)
        {
            DoEnter(manager);
        }

        public override void OnManagedStateMachineVerifyExit(
            ControlledStateManager manager,
            ManagedStateMachineEventArgs args)
        {
            DoVerifyExit();
        }

        public override void OnManagedStateMachineExit(
            ControlledStateManager manager,
            ManagedStateMachineEventArgs args)
        {
            DoExit(manager);
        }
    }
}