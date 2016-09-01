using RoaringFangs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityObject = UnityEngine.Object;

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