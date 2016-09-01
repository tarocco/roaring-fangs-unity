using RoaringFangs;
using UnityEngine;

namespace RoaringFangs.SceneManagement
{
    public interface ISceneHandler : ISceneHandlerBase
    {
        string SceneName { get; }

        SceneLoadCompleteEvent LoadComplete { get; }
        SceneUnloadCompleteEvent UnloadComplete { get; }
    }
}