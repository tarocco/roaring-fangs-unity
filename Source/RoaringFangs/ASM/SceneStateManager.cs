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
    [RequireComponent(typeof(Animator))]
    public class SceneStateManager : ControlledStateManager
    {
        private enum EInitializationState
        {
            Setup,
            Ready
        }

        public enum EStartMode
        {
            None,
            UnloadUnusedScenes,
            UnloadAllOtherScenes
        }

#if ODIN_INSPECTOR

        [TitleGroup("Settings")]
#endif
        [SerializeField]
        private EStartMode _StartMode = EStartMode.UnloadUnusedScenes;

        public EStartMode StartMode
        {
            get { return _StartMode; }
            set
            {
                if (Application.isPlaying)
                    throw new InvalidOperationException(
                        "Cannot change start mode while playing.");
                _StartMode = value;
            }
        }

        private EInitializationState _InitializationState =
            EInitializationState.Setup;

        private EInitializationState InitializationState
        {
            get { return _InitializationState; }
            set { _InitializationState = value; }
        }

        private List<string> _ScenesToUnloadAtStart = new List<string>();

        private List<string> ScenesToUnloadAtStart
        {
            get { return _ScenesToUnloadAtStart; }
            set { _ScenesToUnloadAtStart = value; }
        }

        public bool SkipAlreadyLoadedScenes
        {
            get
            {
                return
                    InitializationState == EInitializationState.Setup &&
                    StartMode == EStartMode.UnloadUnusedScenes;
            }
        }

        private IEnumerable<string> GetOtherSceneNames()
        {
            return Enumerable.Range(0, SceneManager.sceneCount)
                .Select(SceneManager.GetSceneAt)
                .Where(s => s != gameObject.scene)
                .Select(s => s.name);
        }

        private IEnumerable ClearScenesToUnload()
        {
            ScenesToUnloadAtStart.Clear();
            yield return null;
        }

        private void EnqueueUnloadScenes(IEnumerable<string> scene_names)
        {
            foreach (var scene_name in scene_names)
                Debug.Log("Will remove " + scene_name);
            EnqueueCoroutine(Scenes.UnloadTogether(scene_names).GetEnumerator());
            EnqueueCoroutine(ClearScenesToUnload().GetEnumerator());
        }

        private void DoUnloadScenes()
        {
            if (!ScenesToUnloadAtStart.Any())
                return;
            EnqueueUnloadScenes(ScenesToUnloadAtStart);
        }

        protected override void VerifyStateEventInfo(
            IManagedStateEventInfo info)
        {
            try
            {
                info.Verify(this);
            }
            catch (AggregateException aggregate_ex)
            {
                var inner_exceptions = new List<Exception>();
                foreach (var ex in aggregate_ex.InnerExceptions)
                {
                    var redundant_ex = ex as RedundantSceneException;
                    if (redundant_ex != null)
                    {
                        // If we are only unloading unused scenes, don't mind
                        // the scenes already being loaded during setup
                        if (SkipAlreadyLoadedScenes)
                        {
                            // Don't unload this scene because it is supposed to be loaded
                            ScenesToUnloadAtStart.Remove(redundant_ex.SceneName);
                            Debug.Log("Keeping " + redundant_ex.SceneName);
                        }
                    }
                    else
                        inner_exceptions.Add(ex);
                }
                if (inner_exceptions.Any())
                    throw new AggregateException("Could not verify event info", aggregate_ex);
            }
        }

        protected override void ProcessStateEventInfo(IManagedStateEventInfo info)
        {
            // If we encountered an update event from setup mode, it means we're "ready"
            if (InitializationState == EInitializationState.Setup &&
                info.Type == ManagedStateEventType.StateUpdate)
            {
                InitializationState = EInitializationState.Ready;
                // And we can unload scenes we don't want to keep
                DoUnloadScenes();
            }
            info.Process(this);
        }

        protected override void Start()
        {
            ScenesToUnloadAtStart = GetOtherSceneNames().ToList();
            switch (StartMode)
            {
                case EStartMode.UnloadAllOtherScenes:
                    DoUnloadScenes();
                    break;
            }
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}