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
    public class SceneState : StateControllerBase
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

            [HorizontalGroup("Value")]
#endif
            public string Value;

#if ODIN_INSPECTOR

            [HorizontalGroup("Value")]
#endif
            public SceneLoadDirectiveType Type;

#if ODIN_INSPECTOR

            [HorizontalGroup("Condition")]
#endif
            public string ConditionParameter;
        }

        [Serializable]
        public struct ConfigurationObjectDirective
        {
#if ODIN_INSPECTOR
            [HorizontalGroup("Object")]
            [HideLabel]
#endif
            public GameObject Object;

#if ODIN_INSPECTOR
            [HorizontalGroup("Object")]
#endif
            public string ConditionParameter;
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

        private string GetParameterValueByName(string parameter_name)
        {
            return CachedControlledStateManager
                .ParameterEntriesLookup[parameter_name]
                .FirstOrDefault();
        }

        private bool GetConditionValueByName(
            string parameter_name)
        {
            // TODO: encapsulate ControlledStateManager.Animator
            return string.IsNullOrEmpty(parameter_name) ||
                CachedControlledStateManager.Animator.GetBool(parameter_name);
        }

        private bool GetSceneDirectiveConditionValue(
            SceneLoadDirective directive)
        {
            return GetConditionValueByName(directive.ConditionParameter);
        }

        private string ResolveSceneName(string value, SceneLoadDirectiveType type)
        {
            switch (type)
            {
                default:
                case SceneLoadDirectiveType.Name:
                    return value;

                case SceneLoadDirectiveType.Parameter:
                    // value is the parameter name in this case
                    return GetParameterValueByName(value);
            }
        }

        private string ResolveSceneName(SceneLoadDirective directive)
        {
            return ResolveSceneName(directive.Value, directive.Type);
        }

        private static bool StringIsNotNullOrEmpty(string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        public IEnumerable<string> FirstScenesEntryLoad
        {
            get
            {
                return _FirstScenesEntryLoad
                    .Where(GetSceneDirectiveConditionValue)
                    .Select(ResolveSceneName)
                    .Where(StringIsNotNullOrEmpty)
                    .ToArray();
            }
        }

        public IEnumerable<string> SecondScenesEntryLoad
        {
            get
            {
                return _SecondScenesEntryLoad
                    .Where(GetSceneDirectiveConditionValue)
                    .Select(ResolveSceneName)
                    .Where(StringIsNotNullOrEmpty)
                    .ToArray();
            }
        }

        public IEnumerable<string> FirstScenesExitUnload
        {
            get
            {
                return _FirstScenesExitUnload
                    .Where(GetSceneDirectiveConditionValue)
                    .Select(ResolveSceneName)
                    .Where(StringIsNotNullOrEmpty)
                    .ToArray();
            }
        }

        public IEnumerable<string> SecondScenesExitUnload
        {
            get
            {
                return _SecondScenesExitUnload
                    .Where(GetSceneDirectiveConditionValue)
                    .Select(ResolveSceneName)
                    .Where(StringIsNotNullOrEmpty)
                    .ToArray();
            }
        }

        public List<ConfigurationObjectDirective> ConfigurationObjects =
            new List<ConfigurationObjectDirective>();

        private IEnumerable OnStateEnterCoroutine(IEnumerable<string> scene_names, bool skip_if_already_loaded)
        {
            foreach (object o in Scenes.LoadTogether(scene_names, skip_if_already_loaded))
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
            foreach (var directive in ConfigurationObjects)
            {
                if (GetConditionValueByName(directive.ConditionParameter))
                    Instantiate(directive.Object);
            }
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
            var inner_exceptions = new List<Exception>();
            foreach (var scene_name in scene_names_to_load)
            {
                Debug.Log("Verifying " + scene_name);
                // Verify that the scenes to be loaded are valid before enqueing the loader coroutine
                if (!Application.CanStreamedLevelBeLoaded(scene_name))
                    throw new ArgumentException("Scene cannot not be loaded: \"" + scene_name + "\"");
                // Verify that none of the scenes to be loaded are already loaded
                // This limitation is caused by SceneManager.SetActiveScene being really BAD
                var scene = SceneManager.GetSceneByName(scene_name);
                if (scene.isLoaded)
                    inner_exceptions.Add(new RedundantSceneException(
                        "Scene cannot be loaded more than once: \"" + scene_name + "\"", scene_name));
            }
            if (inner_exceptions.Any())
                throw new AggregateException("Could not verify enter", inner_exceptions.ToArray());
        }

        protected void DoVerifyExit()
        {
            var scene_names_to_unload = FirstScenesExitUnload
                .Concat(SecondScenesExitUnload)
                .ToArray();
            // Verify that the scenes to be unloaded are valid before enqueing the unloader coroutine
            var inner_exceptions = new List<Exception>();
            foreach (var scene_name in scene_names_to_unload)
                if (!Application.CanStreamedLevelBeLoaded(scene_name))
                    inner_exceptions.Add(new ArgumentException(
                        "Scene cannot not be unloaded: \"" + scene_name + "\""));
            if (inner_exceptions.Any())
                throw new AggregateException("Could not verify exit", inner_exceptions.ToArray());
        }

        protected void DoEnter(ControlledStateManager manager, bool skip_if_already_loaded)
        {
            var scene_names_to_load = FirstScenesEntryLoad
                .Concat(SecondScenesEntryLoad)
                .ToArray();
            //manager.EnqueueDeferredCoroutine(OnStateEnterCoroutine(scene_names_to_load).GetEnumerator());
            manager.EnqueueCoroutine(OnStateEnterCoroutine(scene_names_to_load, skip_if_already_loaded).GetEnumerator());
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
            var instances = new List<ConfigurationObjectDirective>();
            foreach (var directive in ConfigurationObjects)
            {
                var configuration_object = directive.Object;
                if (configuration_object == null)
                {
                    Debug.LogWarning("Configuration object is null");
                    continue;
                }
                // Parent the instance to the Configuration Object Cache
                var instance = Instantiate(configuration_object, manager.ConfigurationObjectCache);
                // Remove "(Clone)" suffix
                instance.name = configuration_object.name;
                // Add it to the list
                var directive_but_stronger = new ConfigurationObjectDirective()
                {
                    Object = instance,
                    ConditionParameter = directive.ConditionParameter
                };
                instances.Add(directive_but_stronger);
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
            var scene_manager = manager as SceneStateManager;
            DoEnter(scene_manager, scene_manager && scene_manager.SkipAlreadyLoadedScenes);
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
            var scene_manager = manager as SceneStateManager;
            DoEnter(scene_manager, scene_manager && scene_manager.SkipAlreadyLoadedScenes);
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