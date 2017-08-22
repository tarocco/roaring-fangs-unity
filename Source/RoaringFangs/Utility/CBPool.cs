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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace RoaringFangs.Utility
{
    /// <summary>
    /// Circular Buffer class for object pooling in Unity
    /// Necessary for performance reasons when aggressive
    /// garbage collection is a serious problem.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public class CBPool :
        MonoBehaviour,
        IGameObjectPool,
        ISerializationCallbackReceiver
    {
        [Serializable]
        public class ConfigureObjectEvent : UnityEvent<GameObject> { }

#if ODIN_INSPECTOR

        [DisableInPlayMode]
#endif
        [SerializeField]
        private Transform _PoolTransform;

        public Transform PoolTransform
        {
            get { return _PoolTransform; }
            set { _PoolTransform = value; }
        }

        /// <summary>
        /// Structure containing information about objects
        /// that are currently active in the object pool
        /// </summary>
        [Serializable]
        private struct ElementInfo
        {
            /// <summary>
            /// Time relative to game start at which the
            /// element will expire and be deactivated
            /// </summary>
            public float Expiry;

            public int BufferIndex;
        }

        /// <summary>
        /// Default element Time To Live (TTL) in seconds
        /// after activation before an object is deactivated
        /// </summary>
        public float ElementTTL = 2.0f;

        private GameObject[] _AvailableObjects;

        //private GameObject[] _ActiveObjects;

        private int _AvailableObjectIndex = 0;

        /// <summary>
        /// Dictionary containing objects in the pool and their
        /// respective data
        /// </summary>
        private Dictionary<GameObject, ElementInfo> PoolData =
            new Dictionary<GameObject, ElementInfo>();

        protected uint _CycleCounter = uint.MinValue;

        protected uint _ReturnCounter = uint.MinValue;

        [SerializeField]
        private ConfigureObjectEvent _InitializeObject;

        /// <summary>
        /// Event raised for each pool element in Awake()
        /// </summary>
        public ConfigureObjectEvent InitializeObject
        {
            get { return _InitializeObject; }
            private set { _InitializeObject = value; }
        }

        private void BufferPoolObjects()
        {
            var objects = PoolTransform
                .Cast<Transform>()
                .Select(t => t.gameObject);
            _AvailableObjects = objects.ToArray();
            //_ActiveObjects = new GameObject[_AvailableObjects.Length];
        }

        protected virtual void Awake()
        {
            BufferPoolObjects();
            foreach (var game_object in _AvailableObjects)
                InitializeObject.Invoke(game_object);
        }

        protected void Update()
        {
            foreach (var key in PoolData.Keys.ToArray())
            {
                if (Time.time > PoolData[key].Expiry)
                {
                    Return(key);
                }
            }
            //Debug.Log(
            //    "Cycles:  " + _CycleCounter + "\n" +
            //    "Returns: " + _ReturnCounter);
        }

        /// <summary>
        /// Cyclically retrieve an object from the pool and set it active
        /// </summary>
        public GameObject Cycle()
        {
            var number_of_available_objects = _AvailableObjects.Length;
            for (int i = 0; i < number_of_available_objects; i++)
            {
                int index = (_AvailableObjectIndex + i) % number_of_available_objects;
                var @object = _AvailableObjects[index];
                if(@object != null)
                {
                    @object.SetActive(true);
                    PoolData[@object] = new ElementInfo()
                    {
                        Expiry = Time.time + ElementTTL,
                        BufferIndex = index
                    };
                    _AvailableObjects[index] = null;
                    //_ActiveObjects[index] = @object;
                    _AvailableObjectIndex = (index + 1) % number_of_available_objects;
                    _CycleCounter++;
                    return @object; 
                }
            }
            throw new Exception("No available objects in pool");
        }

        /// <summary>
        /// Cyclically retrieve an object from the pool and set it active
        /// </summary>
        public GameObject Cycle(out uint number)
        {
            number = _CycleCounter;
            return Cycle();
        }

        public void Return(GameObject @object)
        {
            if (!PoolData.ContainsKey(@object))
                throw new InvalidOperationException(
                    "Cannot return an object to the pool that was not cycled from it");
            @object.SetActive(false);
            var info = PoolData[@object];
            //_ActiveObjects[info.BufferIndex] = null;
            _AvailableObjects[info.BufferIndex] = @object;
            PoolData.Remove(@object);
            _ReturnCounter++;
        }

        public void OnBeforeSerialize()
        {
            if (PoolTransform == null)
                PoolTransform = transform;
        }

        public void OnAfterDeserialize()
        {
        }
    }
}