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
using UnityEngine;
using UnityEngine.SceneManagement;

#if LIGHTSTRIKE_ADVANCED_INSPECTOR

using AdvancedInspector;

#endif

namespace RoaringFangs.ASM
{
#if LIGHTSTRIKE_ADVANCED_INSPECTOR

    public class SceneState : AIStateMachineBehaviour, IStateController
#else
    public class SceneState : StateMachineBehaviour, IStateController
#endif
    {
        protected ControlledStateManager Manager { get; set; }

        public string ActiveSceneName;

        public List<string> FirstScenesEntryLoad;
        public List<string> SecondScenesEntryLoad;

        public List<string> FirstScenesExitUnload;
        public List<string> SecondScenesExitUnload;

        public List<GameObject> ConfigurationObjects;

        private IEnumerable OnStateEnterCoroutine()
        {
            foreach (object o in Scenes.LoadTogether(FirstScenesEntryLoad))
                yield return o;
            foreach (object o in Scenes.LoadTogether(SecondScenesEntryLoad))
                yield return o;
            if (String.IsNullOrEmpty(ActiveSceneName))
            {
                Debug.LogWarning("No active scene name specified");
            }
            else
            {
                Scene active_scene = SceneManager.GetSceneByName(ActiveSceneName);
                if (!active_scene.IsValid())
                    throw new InvalidOperationException("Specified active scene is invalid\nScene name: \"" + ActiveSceneName + "\"");
                if (!active_scene.isLoaded)
                    Debug.LogWarning("Specified active scene is valid but not yet loaded\nScene name:\"" + ActiveSceneName + "\"");
                SceneManager.SetActiveScene(active_scene);
            }
            foreach (var config_object in ConfigurationObjects)
                Instantiate(config_object);
        }

        private IEnumerable OnStateExitCoroutine()
        {
            foreach (object o in Scenes.UnloadTogether(FirstScenesExitUnload))
                yield return o;
            foreach (object o in Scenes.UnloadTogether(SecondScenesExitUnload))
                yield return o;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo state_info, int layer_index)
        {
            Manager.StartCoroutine(OnStateEnterCoroutine().GetEnumerator());
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo state_info, int layer_index)
        {
            Manager.StartCoroutine(OnStateExitCoroutine().GetEnumerator());
        }

        public void Initialize(ControlledStateManager manager)
        {
            // Assign the manager object
            Manager = manager;
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
    }
}