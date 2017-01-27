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

namespace RoaringFangs.ASM
{
    public class SceneEventHandler : MonoBehaviour, IStateHandler, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string _Name;

        [SerializeField, HideInInspector]
        private int _NameHash;

        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                _NameHash = Animator.StringToHash(value);
            }
        }

        public int NameHash
        {
            get { return _NameHash; }
        }

        [SerializeField]
        private string _Tag;

        [SerializeField, HideInInspector]
        private int _TagHash;

        public string Tag
        {
            get { return _Tag; }
            set
            {
                _Tag = value;
                _TagHash = Animator.StringToHash(value);
            }
        }

        public int TagHash
        {
            get { return _TagHash; }
        }

        public string ActiveSceneName;

        public List<string> FirstScenesEntryLoad;
        public List<string> SecondScenesEntryLoad;

        public List<string> FirstScenesExitUnload;
        public List<string> SecondScenesExitUnload;

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
                    throw new InvalidOperationException("Specified active scene is invalid\nScene name: \"" +
                                                        ActiveSceneName + "\"");
                if (!active_scene.isLoaded)
                    throw new InvalidOperationException(
                        "Specified active scene is valid but not loaded\nScene name:\"" + ActiveSceneName + "\"");
                SceneManager.SetActiveScene(active_scene);
            }
        }

        private IEnumerable OnStateExitCoroutine()
        {
            foreach (object o in Scenes.UnloadTogether(FirstScenesExitUnload))
                yield return o;
            foreach (object o in Scenes.UnloadTogether(SecondScenesExitUnload))
                yield return o;
        }

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public void OnStateEnter(Animator animator, AnimatorStateInfo state_info, int layer_index)
        {
            StartCoroutine(OnStateEnterCoroutine().GetEnumerator());
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public void OnStateExit(Animator animator, AnimatorStateInfo state_info, int layer_index)
        {
            StartCoroutine(OnStateExitCoroutine().GetEnumerator());
        }

        public void OnStateUpdate(Animator animator, AnimatorStateInfo state_info, int layer_index)
        {
        }

        public void OnBeforeSerialize()
        {
            Name = Name;
            Tag = Tag;
        }

        public void OnAfterDeserialize()
        {
            Name = Name;
            Tag = Tag;
        }
    }
}