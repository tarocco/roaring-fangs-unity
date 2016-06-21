using UnityEngine;

using RoaringFangs.Utility;

namespace RoaringFangs
{
    public abstract class ConfiguratorBase : MonoBehaviour
    {
        public bool RunAtStart = true;

        protected virtual void Start()
        {
            if (RunAtStart)
                Run();
        }

        protected abstract void Run();

        public virtual void OnSceneLoadCompleteRun(object sender, SceneLoader.SceneLoadCompleteEventArgs args)
        {
            Run();
        }
    }
}