using RoaringFangs;
using UnityEngine;

namespace RoaringFangs.SceneManagement
{
    public interface ISceneHandlerBase
    {
        void StartLoadAsync(MonoBehaviour self);
        void StartUnloadAsync(MonoBehaviour self);
    }
}