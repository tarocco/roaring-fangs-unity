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

using RoaringFangs.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace RoaringFangs.ASM
{
    [RequireComponent(typeof(Animator))]
    public class ControlledStateManager : MonoBehaviour, ISerializationCallbackReceiver
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

        protected interface IManagedStateEventInfo
        {
            void Process(ControlledStateManager manager);

            void Verify(ControlledStateManager manager);
        }

        protected struct ManagedStateEventInfo<TArgs> : IManagedStateEventInfo
        {
            public readonly TArgs Args;

            // An event type is not used in this case because handlers with different event args will need to be processed
            public readonly Action<ControlledStateManager, TArgs> Handler;

            public readonly UnityAction<object, TArgs> SharedEventHandler;

            /// <summary>
            /// Used for exception-raising state invariants (allows for transactions / rollback operations)
            /// </summary>
            public readonly Action<ControlledStateManager, TArgs> Verifier;

            public void Process(ControlledStateManager manager)
            {
                Handler(manager, Args);
                SharedEventHandler.Invoke(manager, Args);
            }

            public void Verify(ControlledStateManager manager)
            {
                Verifier(manager, Args);
            }

            public ManagedStateEventInfo(
                Action<ControlledStateManager, TArgs> verifier,
                Action<ControlledStateManager, TArgs> handler,
                UnityAction<object, TArgs> shared_event_handler,
                TArgs args)
            {
                Verifier = verifier;
                Handler = handler;
                SharedEventHandler = shared_event_handler;
                Args = args;
            }
        }

        #endregion Classes / Structs / Interfaces

        #region Backing Fields

        private HashSet<IStateController> _ActiveStateControllersSet = new HashSet<IStateController>();

        [SerializeField]
        private Animator _Animator;

        [SerializeField]
        private Transform _ConfigurationObjectCache;

        private Queue<IEnumerator> _CoroutineQueue = new Queue<IEnumerator>();

        private int _LastGoodStatePathHash;

        private Queue<Action> _PreEventActionsQueue = new Queue<Action>();

        private Queue<IManagedStateEventInfo> _StateEventQueue = new Queue<IManagedStateEventInfo>();

        [SerializeField, HideInInspector]
        private List<MonoBehaviour> _StateHandlers;

        [SerializeField]
        private bool _UnloadScenesAtStart;

        private bool _AcceptStateMachineStateEvents;

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
        public int LastGoodStatePathHash
        {
            get { return _LastGoodStatePathHash; }
            private set { _LastGoodStatePathHash = value; }
        }

        public IEnumerable<IStateHandler> StateHandlers
        {
            get { return _StateHandlers.Cast<IStateHandler>(); }
            set
            {
                _StateHandlers = value.Cast<MonoBehaviour>().ToList();
                StateHandlersNameHashLookup = value.ToLookup(h => h.NameHash);
                StateHandlersTagHashLookup = value.ToLookup(h => h.TagHash);
                StateHandlersNameLookup = value.ToLookup(h => h.Name);
                StateHandlersTagLookup = value.ToLookup(h => h.Tag);
            }
        }

        public ILookup<int, IStateHandler> StateHandlersNameHashLookup { get; private set; }

        public ILookup<string, IStateHandler> StateHandlersNameLookup { get; private set; }

        public ILookup<int, IStateHandler> StateHandlersTagHashLookup { get; private set; }

        public ILookup<string, IStateHandler> StateHandlersTagLookup { get; private set; }

        public bool UnloadScenesAtStart
        {
            get { return _UnloadScenesAtStart; }
            set { _UnloadScenesAtStart = value; }
        }

        protected HashSet<IStateController> ActiveStateControllersSet
        {
            get { return _ActiveStateControllersSet; }
            private set { _ActiveStateControllersSet = value; }
        }

        /// <summary>
        /// Animator used by this manager
        /// </summary>
        protected Animator Animator
        {
            get { return _Animator; }
            private set { _Animator = value; }
        }

        protected Queue<IEnumerator> CoroutineQueue
        {
            get { return _CoroutineQueue; }
        }

        /// <summary>
        /// Queue of actions performed just before managed state events afer verification passes
        /// </summary>
        protected Queue<Action> PreEventActionsQueue
        {
            get { return _PreEventActionsQueue; }
        }

        protected Queue<IManagedStateEventInfo> StateEventQueue
        {
            get { return _StateEventQueue; }
        }

        #endregion Properties

        #region Events

        [SerializeField]
        private ManagedStateEvent _AnyStateEntry;

        [SerializeField]
        private ManagedStateEvent _AnyStateExit;

        [SerializeField]
        private ManagedStateEvent _AnyStateUpdate;

        [SerializeField]
        private ManagedStateMachineEvent _AnyStateMachineEntry;

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

        public bool AcceptStateMachineStateEvents
        {
            get { return _AcceptStateMachineStateEvents; }
            protected set { _AcceptStateMachineStateEvents = value; }
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

        /// <summary>
        /// Calls Animator.Update and clears state event queues
        /// </summary>
        /// <param name="delta_time"></param>
        public void FlushAnimator(float delta_time)
        {
            Animator.Update(delta_time);
            ClearStateEventInfoQueues();
        }

        public AnimatorTransitionInfo GetAnimatorTransitionInfo(int layer_index)
        {
            return Animator.GetAnimatorTransitionInfo(layer_index);
        }

        public bool IsAnimatorInTransition(int layer_index)
        {
            return Animator.IsInTransition(layer_index);
        }

        public void OnAfterDeserialize()
        {
            StateHandlers = StateHandlers;
        }

        public void OnBeforeSerialize()
        {
            Animator = GetComponent<Animator>();
            StateHandlers = GetComponents<IStateHandler>();
        }

        public void OnStateControllerEntry(object sender, StateControllerEventArgs args)
        {
            var state_controller = sender as IStateController;
            ActiveStateControllersSet.Add(state_controller);
            // TODO
            /*
            var info = args.AnimatorStateInfo;
            var state_handlers = StateHandlersNameHashLookup[info.shortNameHash];
            foreach (var state_handler in state_handlers)
                state_handler.OnManagedStateEnter(this, args.Animator, args.AnimatorStateInfo, args.LayerIndex);
            AnyStateEntry.Invoke(this, args);
            */

            var info = new ManagedStateEventInfo<ManagedStateEventArgs>(
                state_controller.OnManagedStateVerifyEnter,
                state_controller.OnManagedStateEnter,
                AnyStateEntry.Invoke,
                new ManagedStateEventArgs(state_controller, args.Animator, args.AnimatorStateInfo, args.LayerIndex));
            StateEventQueue.Enqueue(info);
        }

        public void OnStateControllerExit(object sender, StateControllerEventArgs args)
        {
            var state_controller = sender as IStateController;
            ActiveStateControllersSet.Remove(state_controller);
            // TODO
            /*
            var info = args.AnimatorStateInfo;
            var state_handlers = StateHandlersNameHashLookup[info.shortNameHash];
            foreach (var state_handler in state_handlers)
            {
                state_handler.OnManagedStateExit(this, args.Animator, args.AnimatorStateInfo, args.LayerIndex);
            }
            */

            var info = new ManagedStateEventInfo<ManagedStateEventArgs>(
                state_controller.OnManagedStateVerifyExit,
                state_controller.OnManagedStateExit,
                AnyStateExit.Invoke,
                new ManagedStateEventArgs(state_controller, args.Animator, args.AnimatorStateInfo, args.LayerIndex));
            StateEventQueue.Enqueue(info);
        }

        public void OnStateControllerUpdate(object sender, StateControllerEventArgs args)
        {
            var state_controller = sender as IStateController;
            // TODO
            /*
            var info = args.AnimatorStateInfo;
            var state_handlers = StateHandlersNameHashLookup[info.shortNameHash];
            foreach (var state_handler in state_handlers)
                state_handler.OnManagedStateUpdate(this, args.Animator, args.AnimatorStateInfo, args.LayerIndex);
            */

            var info = new ManagedStateEventInfo<ManagedStateEventArgs>(
                state_controller.OnManagedStateVerifyUpdate,
                state_controller.OnManagedStateUpdate,
                AnyStateUpdate.Invoke,
                new ManagedStateEventArgs(state_controller, args.Animator, args.AnimatorStateInfo, args.LayerIndex));
            StateEventQueue.Enqueue(info);
        }

        public void OnStateMachineControllerEntry(object sender, StateMachineControllerEventArgs args)
        {
            var state_controller = sender as IStateController;

            var info = new ManagedStateEventInfo<ManagedStateMachineEventArgs>(
                state_controller.OnManagedStateMachineVerifyEnter,
                state_controller.OnManagedStateMachineEnter,
                AnyStateMachineEntry.Invoke,
                new ManagedStateMachineEventArgs(state_controller, args.Animator, args.StateMachinePathHash));
            StateEventQueue.Enqueue(info);
        }

        public void OnStateMachineControllerExit(object sender, StateMachineControllerEventArgs args)
        {
            var state_controller = sender as IStateController;

            var info = new ManagedStateEventInfo<ManagedStateMachineEventArgs>(
                state_controller.OnManagedStateMachineVerifyExit,
                state_controller.OnManagedStateMachineExit,
                AnyStateMachineExit.Invoke,
                new ManagedStateMachineEventArgs(state_controller, args.Animator, args.StateMachinePathHash));
            StateEventQueue.Enqueue(info);
        }

        /// <summary>
        /// Calls <see cref="Animator.Update"/> and processes state event queues
        /// </summary>
        /// <param name="delta_time">Used for <see cref="Animator.Update"/></param>
        public void Process(float delta_time)
        {
            Animator.Update(delta_time);
            SafelyProcessEventQueue();
        }

        public void ResetAnimatorTrigger(string name)
        {
            Animator.ResetTrigger(name);
        }

        /// <summary>
        /// Sets the trigger <paramref name="name"/> on the animator component,
        /// updates the animator with a zero time update, verifies the
        /// resulting state event queue, and processes it if it verifies.
        /// </summary>
        /// <param name="name">The name of the trigger to set on the animator.</param>
        public void SetAnimatorTrigger(string name)
        {
            Animator.SetTrigger(name);
        }

        /// <summary>
        /// Clears the state event info queues
        /// </summary>
        protected void ClearStateEventInfoQueues()
        {
            PreEventActionsQueue.Clear();
            StateEventQueue.Clear();
        }

        /// <summary>
        /// Verifies and (if successfully verified) processes the state event queue
        /// </summary>
        protected void ProcessAllStateEventQueue()
        {
            //var all_event_queues = StateExitEventQueue.Concat(StateEnterEventQueue).Concat(StateUpdateEventQueue);
            ProcessActionQueue(PreEventActionsQueue);
            VerifyStateEventInfo(StateEventQueue);
            ProcessStateEventInfoQueue(StateEventQueue);
        }

        protected void SafelyProcessEventQueue()
        {
            try
            {
                ProcessAllStateEventQueue();
            }
            catch (Exception ex)
            {
                Animator.Play(LastGoodStatePathHash);
                Animator.Update(0.0f);
                ClearStateEventInfoQueues();
                throw ex;
            }

            // The processing succeeded and the last good state should be the current state
            // TODO: specify layer index?
            var animator_state_info = Animator.GetCurrentAnimatorStateInfo(0);
            LastGoodStatePathHash = animator_state_info.fullPathHash;
        }

        /// <summary>
        /// Dequeues and invokes all of the <see cref="Actions"/> in <paramref name="queue"/>.
        /// </summary>
        /// <param name="queue"></param>
        private void ProcessActionQueue(Queue<Action> queue)
        {
            while (queue.Count > 0)
                queue.Dequeue()();
        }

        private IEnumerable ProcessCoroutineQueue()
        {
            for (;;)
            {
                // TODO: don't assume layer 0?
                IEnumerator coroutine;
                if (CoroutineQueue.Count > 0)
                {
                    coroutine = CoroutineQueue.Dequeue().GetSafeCoroutine();
                    while (coroutine.MoveNext() && coroutine.Current != null)
                        yield return coroutine.Current;
                }
                else
                    yield return null;
            }
        }

        private void ProcessStateEventInfoQueue(
                    Queue<IManagedStateEventInfo> queue)
        {
            while (queue.Count > 0)
                queue.Dequeue().Process(this);
        }

        private void Start()
        {
            var state_controllers = Animator.GetBehaviours<StateMachineBehaviour>()
                .OfType<IStateController>()
                .ToArray();
            foreach (var state_controller in state_controllers)
                state_controller.Initialize(this);

            if (UnloadScenesAtStart)
            {
                var other_scene_names = Enumerable.Range(0, SceneManager.sceneCount)
                    .Select(SceneManager.GetSceneAt)
                    .Where(s => s != gameObject.scene)
                    .Select(s => s.name);
                EnqueueCoroutine(Scenes.UnloadTogether(other_scene_names).GetEnumerator());
            }

            StartCoroutine(ProcessCoroutineQueue().GetEnumerator());

            // HACK: Enable this here in order to capture StateMachineController OnStateEnter/Exit events only once at the start
            AcceptStateMachineStateEvents = true;
        }

        private void Update()
        {
            SafelyProcessEventQueue();
            // HACK: Disable this here in order to stop capturing StateMachineController OnStateEnter/Exit events -- only the first set of calls from the animator should include these
            AcceptStateMachineStateEvents &= !ActiveStateControllers.Any();
        }

        /// <summary>
        /// Verifies that the state event handlers can be processed without throwing exceptions.
        /// Verifiers should throw exceptions if a precondition (or other invariant) is not met.
        /// </summary>
        /// <param name="queue"></param>
        private void VerifyStateEventInfo(
            IEnumerable<IManagedStateEventInfo> queue)
        {
            foreach (var info in queue)
                info.Verify(this);
        }
    }
}