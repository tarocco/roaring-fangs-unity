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

namespace RoaringFangs.SceneManagement
{
    [Serializable]
    public class SceneHandlerGroup :
        SceneHandlerGroup<ISceneHandler>,
        ISceneHandlerGroup<ISceneHandler>
    {
        //public event SceneLoadManyCompletedHandler SceneLoadManyCompleted;
        //public event SceneUnloadManyCompletedHandler SceneUnloadManyCompleted;

        [SerializeField]
        private List<ISceneHandler> _SceneHandlers;

        public override IEnumerable<ISceneHandler> SceneHandlers
        {
            get
            {
                return _SceneHandlers;
            }
            protected set
            {
                _SceneHandlers = value.ToList();
            }
        }

        public void AddSceneHandlers(params ISceneHandler[] handlers)
        {
            _SceneHandlers.AddRange(handlers);
        }

        public void RemoveSceneHandler(ISceneHandler handler)
        {
            _SceneHandlers.Remove(handler);
        }

        public SceneHandlerGroup(params ISceneHandler[] scene_handlers)
        {
            SceneHandlers = new List<ISceneHandler>(scene_handlers);
        }
    }
}