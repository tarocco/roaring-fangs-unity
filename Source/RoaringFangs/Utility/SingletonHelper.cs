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

using UnityEngine;

namespace RoaringFangs.Utility
{
    public class SingletonHelper : MonoBehaviour
    {
        [SerializeField]
        private GameObject _Prefab;

        private GameObject _Instance;

        [SerializeField]
        private bool _DontDestroy = true;

        public bool DontDestroy
        {
            get { return _DontDestroy; }
            set { _DontDestroy = value; }
        }

        private static bool _singletonInstantiated;

        private void Awake()
        {
            //var helpers = FindObjectsOfType<SingletonHelper>();

            // if every helper is this helper, or every helper's same doesn't match this helper's name, then instantiate
            // u wot m8
            // I'm just gonna leave it there because I don't get it
            //if (helpers.All(s => s == this || s.name != name))
            //{
            if (_singletonInstantiated)
                return;

            _Instance = (GameObject)GameObject.Instantiate(_Prefab);
            if (_DontDestroy)
                DontDestroyOnLoad(_Instance);

            _singletonInstantiated = true;
            //}
        }
    }
}