using RoaringFangs.Utility;
using UnityEngine;

namespace RoaringFangs
{
    public abstract class ConfiguratorBase : MonoBehaviour
    {
        public bool RunAtStart = false;

        protected virtual void Start()
        {
            if (RunAtStart)
                Run();
        }

        public abstract void Run();

        public virtual void OnSceneLoadCompleteRun(object sender, SceneLoadCompleteEventArgs args)
        {
            Run();
        }
    }
}