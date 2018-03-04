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
using RoaringFangs.Utility;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace RoaringFangs.ASM
{
    [RequireComponent(typeof(Animator))]
    public partial class ControlledStateManager : MonoBehaviour, ISerializationCallbackReceiver
    {
        #region Delegates

        protected delegate void ManagedStateEventHandler(
            ControlledStateManager manager,
            ManagedStateEventArgs args);

        protected delegate void ManagedStateMachineEventHandler(
            ControlledStateManager manager,
            ManagedStateMachineEventArgs args);

        #endregion Delegates

        #region Classes / Structs / Interfaces

        [Serializable]
        protected struct ParameterEntry
        {
#if ODIN_INSPECTOR

            [HorizontalGroup]
#endif
            public string Key;

#if ODIN_INSPECTOR

            [HorizontalGroup]
#endif
            public string Value;

            public ParameterEntry(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }

        #endregion Classes / Structs / Interfaces

        #region Backing Fields

        private HashSet<IStateController> _ActiveStateControllersSet = new HashSet<IStateController>();

#if ODIN_INSPECTOR

        [TitleGroup("References", indent: false, order: -1)]
#endif
        [SerializeField]
        private Animator _Animator;

#if ODIN_INSPECTOR

        [TitleGroup("References")]
#endif
        [SerializeField]
        private Transform _ConfigurationObjectCache;

        private Queue<IEnumerator> _CoroutineQueue = new Queue<IEnumerator>();

        private int _LastGoodStatePathHash;

        private Queue<IManagedStateEventInfo> _StateEventInfoQueue = new Queue<IManagedStateEventInfo>();

        #endregion Backing Fields

        #region Properties

        /// <summary>
        /// State controllers currently entered by the Animator state machine's current state
        /// </summary>
        public IEnumerable<IStateController> ActiveStateControllers
        {
            get { return ActiveStateControllersSet; }
        }

        /// <summary>
        /// Game object to store runtime instances of prefabs so that they
        /// may be mutated without affecting the project's asset prefabs
        /// (It's a Unity editor thing, seriously)
        /// </summary>
        public Transform ConfigurationObjectCache
        {
            get { return _ConfigurationObjectCache; }
            set { _ConfigurationObjectCache = value; }
        }

        /// <summary>
        /// Path hash of the last animator state this manager was able to successfully enter.
        /// </summary>
        protected int LastGoodStatePathHash
        {
            get { return _LastGoodStatePathHash; }
            private set { _LastGoodStatePathHash = value; }
        }

#if ODIN_INSPECTOR

        [TitleGroup("Settings", indent: false)]
        [PropertyOrder(100)]
#endif
        [SerializeField]
        private ParameterEntry[] _ParameterEntries;

        private ILookup<string, string> _ParameterEntriesLookup;

        public ILookup<string, string> ParameterEntriesLookup
        {
            get
            {
                if (_ParameterEntriesLookup == null)
                    ParameterEntriesLookup = _ParameterEntries
                        .ToLookup(e => e.Key, e => e.Value);
                return _ParameterEntriesLookup;
            }
            private set
            {
                _ParameterEntriesLookup = value;
                _ParameterEntries = ParameterEntriesLookup
                    .SelectMany(g => g, (g, e) => new ParameterEntry(g.Key, e))
                    .ToArray();
            }
        }

        protected HashSet<IStateController> ActiveStateControllersSet
        {
            get { return _ActiveStateControllersSet; }
            //private set { _ActiveStateControllersSet = value; }
        }

        /// <summary>
        /// Animator used by this manager
        /// </summary>
        public Animator Animator
        {
            get { return _Animator; }
            private set { _Animator = value; }
        }

        protected Queue<IEnumerator> CoroutineQueue
        {
            get { return _CoroutineQueue; }
        }

        protected Queue<IManagedStateEventInfo> StateEventInfoQueue
        {
            get { return _StateEventInfoQueue; }
        }

        #endregion Properties

        #region Events

#if ODIN_INSPECTOR

        [TitleGroup("Events", indent: false, order: 100)]
#endif
        [SerializeField]
        private ManagedStateEvent _AnyStateEntry;

#if ODIN_INSPECTOR

        [TitleGroup("Events")]
#endif
        [SerializeField]
        private ManagedStateEvent _AnyStateExit;

#if ODIN_INSPECTOR

        [TitleGroup("Events")]
#endif
        [SerializeField]
        private ManagedStateEvent _AnyStateUpdate;

#if ODIN_INSPECTOR

        [TitleGroup("Events")]
#endif
        [SerializeField]
        private ManagedStateMachineEvent _AnyStateMachineEntry;

#if ODIN_INSPECTOR

        [TitleGroup("Events")]
#endif
        [SerializeField]
        private ManagedStateMachineEvent _AnyStateMachineExit;

        public ManagedStateEvent AnyStateEntry
        {
            get { return _AnyStateEntry; }
            private set { _AnyStateEntry = value; }
        }

        public ManagedStateEvent AnyStateExit
        {
            get { return _AnyStateExit; }
            private set { _AnyStateExit = value; }
        }

        public ManagedStateEvent AnyStateUpdate
        {
            get { return _AnyStateUpdate; }
            private set { _AnyStateUpdate = value; }
        }

        public ManagedStateMachineEvent AnyStateMachineEntry
        {
            get { return _AnyStateMachineEntry; }
            private set { _AnyStateMachineEntry = value; }
        }

        public ManagedStateMachineEvent AnyStateMachineExit
        {
            get { return _AnyStateMachineExit; }
            private set { _AnyStateMachineExit = value; }
        }

        #endregion Events

        /// <summary>
        /// Enqueue a coroutine to be processed by this manager
        /// </summary>
        /// <param name="coroutine"></param>
        public void EnqueueCoroutine(IEnumerator coroutine)
        {
            CoroutineQueue.Enqueue(coroutine);
        }

        public AnimatorTransitionInfo GetAnimatorTransitionInfo(int layer_index)
        {
            return Animator.GetAnimatorTransitionInfo(layer_index);
        }

        public bool IsAnimatorInTransition(int layer_index)
        {
            return Animator.IsInTransition(layer_index);
        }

        public void SetParameter(string key, string value)
        {
            var entry = new ParameterEntry(key, value);
            var updated = false;
            var entries = _ParameterEntries
                .Select((p) =>
                    {
                        if (p.Key != key || updated)
                            return p;
                        updated = true;
                        return entry;
                    });
            // Force calculation of updated flag
            entries = entries.ToArray();
            if (!updated)
                entries = entries.Concat(new[] { entry });
            _ParameterEntries = entries.ToArray();
            ParameterEntriesLookup = null;
        }

        public void OnBeforeSerialize()
        {
            Animator = GetComponent<Animator>();

            // Force round-trip update for validation
            ParameterEntriesLookup = null;
        }

        public void OnAfterDeserialize()
        {
        }

        public void OnStateControllerEntry(object sender, StateControllerEventArgs args)
        {
            var state_controller = (IStateController)sender;
            ActiveStateControllersSet.Add(state_controller);

            var info = new ManagedStateEventInfo<ManagedStateEventArgs>(
                ManagedStateEventType.StateEntry,
                state_controller.OnManagedStateVerifyEnter,
                state_controller.OnManagedStateEnter,
                AnyStateEntry.Invoke,
                new ManagedStateEventArgs(state_controller, args.Animator, args.AnimatorStateInfo, args.LayerIndex));
            StateEventInfoQueue.Enqueue(info);
        }

        public void OnStateControllerExit(object sender, StateControllerEventArgs args)
        {
            var state_controller = (IStateController)sender;
            ActiveStateControllersSet.Remove(state_controller);

            var info = new ManagedStateEventInfo<ManagedStateEventArgs>(
                ManagedStateEventType.StateExit,
                state_controller.OnManagedStateVerifyExit,
                state_controller.OnManagedStateExit,
                AnyStateExit.Invoke,
                new ManagedStateEventArgs(state_controller, args.Animator, args.AnimatorStateInfo, args.LayerIndex));
            StateEventInfoQueue.Enqueue(info);
        }

        public void OnStateControllerUpdate(object sender, StateControllerEventArgs args)
        {
            var state_controller = (IStateController)sender;

            var info = new ManagedStateEventInfo<ManagedStateEventArgs>(
                ManagedStateEventType.StateUpdate,
                state_controller.OnManagedStateVerifyUpdate,
                state_controller.OnManagedStateUpdate,
                AnyStateUpdate.Invoke,
                new ManagedStateEventArgs(state_controller, args.Animator, args.AnimatorStateInfo, args.LayerIndex));
            StateEventInfoQueue.Enqueue(info);
        }

        public void OnStateMachineControllerEntry(object sender, StateMachineControllerEventArgs args)
        {
            var state_controller = (IStateController)sender;
            ActiveStateControllersSet.Add(state_controller);
            var info = new ManagedStateEventInfo<ManagedStateMachineEventArgs>(
                ManagedStateEventType.StateMachineEntry,
                state_controller.OnManagedStateMachineVerifyEnter,
                state_controller.OnManagedStateMachineEnter,
                AnyStateMachineEntry.Invoke,
                new ManagedStateMachineEventArgs(state_controller, args.Animator, args.StateMachinePathHash));
            StateEventInfoQueue.Enqueue(info);
        }

        public void OnStateMachineControllerExit(object sender, StateMachineControllerEventArgs args)
        {
            var state_controller = (IStateController)sender;
            ActiveStateControllersSet.Remove(state_controller);
            var info = new ManagedStateEventInfo<ManagedStateMachineEventArgs>(
                ManagedStateEventType.StateMachineExit,
                state_controller.OnManagedStateMachineVerifyExit,
                state_controller.OnManagedStateMachineExit,
                AnyStateMachineExit.Invoke,
                new ManagedStateMachineEventArgs(state_controller, args.Animator, args.StateMachinePathHash));
            StateEventInfoQueue.Enqueue(info);
        }

        /// <summary>
        /// Calls <see cref="Animator.Update"/> and processes state event queues
        /// </summary>
        /// <param name="delta_time">Used for <see cref="Animator.Update"/></param>
        public void Process(float delta_time)
        {
            // Animator.Update needs to be called twice because otherwise
            // Unity will, for whatever reason, make duplicate calls on OnState*
            // methods of StateMachineBehaviors
            var update_mode = Animator.updateMode;
            Animator.updateMode = AnimatorUpdateMode.Normal;
            Animator.Update(delta_time);
            SafelyProcessEventQueue();
            //SafelyProcessEventQueue();
            Animator.Update(0.0f);
            Animator.updateMode = update_mode;
        }

        public void ResetAnimatorTrigger(string name)
        {
            Animator.ResetTrigger(name);
        }

        /// <summary>
        /// Sets the trigger <paramref name="name"/> on the animator component.
        /// </summary>
        /// <param name="name">The name of the trigger to set on the animator.</param>
        public void SetAnimatorTrigger(string name)
        {
            Animator.SetTrigger(name);
        }

        protected void ProcessStateEventInfos(IEnumerable<IManagedStateEventInfo> infos)
        {
            foreach (var info in infos)
                ProcessStateEventInfo(info);
        }

        protected void SafelyProcessEventQueue()
        {
            IEnumerable<IManagedStateEventInfo> verified_infos;
            try
            {
                verified_infos =
                    GetVerifiedStateEventInfos(StateEventInfoQueue)
                    .ToArray();
            }
            catch (Exception ex)
            {
                Animator.Play(LastGoodStatePathHash);
                StateEventInfoQueue.Clear();
                throw ex;
            }

            try
            {
                // If any exceptions are thrown here, we will be in an unknown state

                // Process will perform the managed actions of the state controller
                ProcessStateEventInfos(verified_infos);

                // The processing succeeded and the last good state should be the current state
                // TODO: specify layer index?
                var animator_state_info = Animator.GetCurrentAnimatorStateInfo(0);
                LastGoodStatePathHash = animator_state_info.fullPathHash;
            }
            catch (Exception ex)
            {
                // Last ditch efforts
                Debug.LogError("CRITICAL ERROR");
                for (var i = 0; i < SceneManager.sceneCount; i++)
#pragma warning disable CS0618 // Type or member is obsolete
                    SceneManager.UnloadScene(i);
#pragma warning restore CS0618 // Type or member is obsolete
                throw ex;
            }
        }

        private IEnumerable ProcessCoroutineQueue()
        {
            for (;;)
            {
                // TODO: don't assume layer 0?
                if (CoroutineQueue.Count > 0)
                {
                    var coroutine = CoroutineQueue.Dequeue().GetSafeCoroutine();
                    while (coroutine.MoveNext() && coroutine.Current != null)
                        yield return coroutine.Current;
                }
                else
                    yield return null;
            }
        }

        protected virtual void Start()
        {
            var state_controllers = Animator.GetBehaviours<StateMachineBehaviour>()
                .OfType<IStateController>()
                .ToArray();
            foreach (var state_controller in state_controllers)
                state_controller.Initialize(this);
            StartCoroutine(ProcessCoroutineQueue().GetEnumerator());
        }

        protected virtual void Update()
        {
            SafelyProcessEventQueue();
        }

        /// <summary>
        /// Verifies that the state event handlers can be processed without throwing exceptions.
        /// Verifiers should throw exceptions if a precondition (or other invariant) is not met.
        /// </summary>
        /// <param name="queue"></param>
        private IEnumerable<IManagedStateEventInfo> GetVerifiedStateEventInfos(
            Queue<IManagedStateEventInfo> queue)
        {
            while (queue.Count > 0)
            {
                var info = queue.Dequeue();
                VerifyStateEventInfo(info);
                yield return info;
            }
        }

        /// <summary>
        /// Verifies and returns whether the managed state info should be processed
        /// </summary>
        /// <exception cref="Exception">
        /// Thrown if the info's state verification fails
        /// </exception>
        protected virtual void VerifyStateEventInfo(IManagedStateEventInfo info)
        {
            info.Verify(this);
        }

        protected virtual void ProcessStateEventInfo(IManagedStateEventInfo info)
        {
            info.Process(this);
        }
    }
}