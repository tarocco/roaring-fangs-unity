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

namespace RoaringFangs.Utility
{
    /// <summary>
    /// Circular Buffer class for object pooling in Unity
    /// Necessary for performance reasons when aggressive
    /// garbage collection is a serious problem.
    /// </summary>
    public class CBPool : MonoBehaviour
    {
        /// <summary>
        /// Structure containing information about objects
        /// that are currently active in the object pool
        /// </summary>
        [Serializable]
        public struct ElementData
        {
            /// <summary>
            /// Time relative to game start at which the
            /// element will expire and be deactivated
            /// </summary>
            public float Expiry;
        }

        /// <summary>
        /// Default element Time To Live (TTL) in seconds
        /// after activation before an object is deactivated
        /// </summary>
        public float ElementTTL = 2.0f;

        /// <summary>
        /// Dictionary containing objects in the pool and their
        /// respective data
        /// </summary>
        protected Dictionary<GameObject, ElementData> PoolData;

        public void SetElementData(GameObject o, ElementData data)
        {
            PoolData[o] = data;
        }

        public ElementData GetElementData(GameObject o)
        {
            return PoolData[o];
        }

        /// <summary>
        /// Enumerable for GameObjects associated with child
        /// transforms of this component's transform
        /// </summary>
        /// <returns>Enumerable over a given child GameObject</returns>
        protected IEnumerable<GameObject> GetPoolElements()
        {
            foreach (Transform t in transform)
            {
                yield return t.gameObject;
            }
        }

        public IEnumerable<GameObject> ActiveElements()
        {
            foreach (GameObject pool_object in PoolData.Keys)
                yield return pool_object;
        }

        protected virtual void Start()
        {
            _Cycle = _Cycle ?? CycleGenerator().GetEnumerator();
            _UpdateCoroutine = _UpdateCoroutine ?? UpdateCoroutine().GetEnumerator();
            PoolData = new Dictionary<GameObject, ElementData>();
        }

        protected void Update()
        {
            _UpdateCoroutine.MoveNext();
        }

        /// <summary>
        /// Enumerator instance from UpdateCoroutine for this component
        /// </summary>
        protected IEnumerator _UpdateCoroutine;

        /// <summary>
        /// Enumerable for updating each element in the PoolData,
        /// namely expiry (deactivation and entry removal)
        /// </summary>
        /// <returns>Cyclic enumerable</returns>
        protected IEnumerable UpdateCoroutine()
        {
            for (;;)
            {
                if (PoolData.Count == 0)
                    yield return null;
                else
                {
                    GameObject[] PoolKeys = PoolData.Keys.ToArray();
                    foreach (var key in PoolKeys)
                    {
                        if (Time.time > PoolData[key].Expiry)
                        {
                            key.SetActive(false);
                            PoolData.Remove(key);
                        }
                        yield return null;
                    }
                }
            }
        }

        /// <summary>
        /// Enumerable for retrieving an object from the pool
        /// Object will be activated before it is returned
        /// </summary>
        /// <returns>
        /// Cyclic enumerable with GameObject that
        /// has just been activated
        /// </returns>
        protected IEnumerable<GameObject> CycleGenerator()
        {
            for (;;)
            {
                var elements = GetPoolElements().ToArray();
                if (elements.Count() > 0)
                {
                    foreach (GameObject game_object in elements)
                    {
                        game_object.SetActive(true);
                        PoolData[game_object] = new ElementData() { Expiry = Time.time + ElementTTL };
                        yield return game_object;
                    }
                }
                else
                    yield return null;
            }
        }

        /// <summary>
        /// Enumerator instance from CycleGenerator for this component
        /// </summary>
        protected IEnumerator<GameObject> _Cycle;

        /// <summary>
        /// Cyclically retrieve an object from the pool and set it active
        /// </summary>
        public GameObject Cycle()
        {
            _Cycle.MoveNext();
            return _Cycle.Current;
        }

        public void Return(GameObject element)
        {
            if (PoolData.ContainsKey(element))
            {
                element.SetActive(false);
                ElementData e = PoolData[element];
                e.Expiry = float.NegativeInfinity;
                PoolData[element] = e;
            }
        }
    }
}