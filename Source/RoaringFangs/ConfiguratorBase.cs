using RoaringFangs.Utility;
using UnityEngine;

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

        public virtual void OnSceneLoadCompleteRun(object sender, Scenes.SceneLoadCompleteEventArgs args)
        {
            Run();
        }
    }
}