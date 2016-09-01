using RoaringFangs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoaringFangs.SceneManagement
{
    public interface ISceneHandlerGroup<TSceneHandler> : ISceneHandlerBase
        where TSceneHandler : ISceneHandler
    {
        IEnumerable<TSceneHandler> SceneHandlers { get; }
    }
}