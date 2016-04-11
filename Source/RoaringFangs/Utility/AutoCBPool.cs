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
using System.Text;

using UnityEngine;

namespace RoaringFangs.Utility
{
    public class AutoCBPool : CBPool
    {
        [SerializeField]
        private bool _FillPoolAtStart = true;
        public bool FillPoolAtStart
        {
            get { return _FillPoolAtStart; }
            set { _FillPoolAtStart = value; }
        }
        [SerializeField]
        private GameObject _Reference;
        public GameObject Reference
        {
            get { return _Reference; }
            set { _Reference = value; }
        }
        [SerializeField]
        private int _PoolSize = 16;
        public int PoolSize
        {
            get { return _PoolSize; }
            set { _PoolSize = value; }
        }
        protected override void Start()
        {
            // How many objects will we need to make?
            // Use the difference between the pool size and objects remaining in the pool
            int pool_size_current = transform.childCount;
            // TODO: debug build assert pool_size_current >= 0 && pool_size_current <= 1
            int number_of_objects_to_instantiate = PoolSize - pool_size_current;
            // Create objects as necessary
            for (int i = 0; i < number_of_objects_to_instantiate; i++)
            {
                var o = GameObject.Instantiate(Reference);
                // Turn off the new object
                o.SetActive(false);
                // Parent objects to the pool
                // Don't restore world matrix and zero locally at pool transform
                o.transform.SetParent(transform, false);
            }
            base.Start();
        }
    }
}
