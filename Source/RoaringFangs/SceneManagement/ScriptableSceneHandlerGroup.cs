using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoaringFangs.SceneManagement
{
    [CreateAssetMenu(
        fileName = "New Scene Handler Group",
        menuName = "Roaring Fangs/Scene Managment/Scene Handler Group")]
    public class ScriptableSceneHandlerGroup : ScriptableObject, ISceneHandlerBase
    {
        [Serializable]
        public class SceneHandlerGroup : SceneHandlerGroup<ScriptableSceneHandler>
        {
            public SceneHandlerGroup(params ScriptableSceneHandler[] scene_handlers) :
                base(scene_handlers)
            {
            }

            [SerializeField]
            //[InlineScriptableSceneHandler]
            private List<ScriptableSceneHandler> _SceneHandlers;

            public override IEnumerable<ScriptableSceneHandler> SceneHandlers
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
        }

        [SerializeField]
        private SceneHandlerGroup _Self;

        public void StartLoadAsync(MonoBehaviour self)
        {
            _Self.StartLoadAsync(self);
        }

        public void StartUnloadAsync(MonoBehaviour self)
        {
            _Self.StartUnloadAsync(self);
        }
    }
}