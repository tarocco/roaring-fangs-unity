using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using System;
using System.Collections;

namespace RoaringFangs.Utility
{
    public static class Scenes
    {
        public enum LoadMode
        {
            Add,
            CheckAdd,
            UnloadAdd,
            ReplaceActive
        }

        public static void Load(string scene_name, LoadMode mode)
        {
            switch (mode)
            {
                default:
                case LoadMode.Add:
                    SceneManager.LoadScene(scene_name, LoadSceneMode.Additive);
                    break;
                case LoadMode.CheckAdd:
                    if (!SceneManager.GetSceneByName(scene_name).isLoaded)
                        SceneManager.LoadScene(scene_name, LoadSceneMode.Additive);
                    // else throw an exception (should it?)
                    break;
                case LoadMode.UnloadAdd:
                    SceneManager.UnloadScene(scene_name);
                    SceneManager.LoadScene(scene_name, LoadSceneMode.Additive);
                    break;
                case LoadMode.ReplaceActive:
                    string scene_originally_active_name = SceneManager.GetActiveScene().name;
                    SceneManager.LoadScene(scene_name, LoadSceneMode.Additive);
                    var scene_loaded = SceneManager.GetSceneByName(scene_name);
                    SceneManager.SetActiveScene(scene_loaded);
                    SceneManager.UnloadScene(scene_originally_active_name);
                    break;
            }
        }
        public static IEnumerable LoadAsync(string scene_name, LoadMode mode)
        {
            // This is here because of a Mono bug...
            YieldInstruction operation = null;
            string scene_originally_active_name = null;
            switch (mode)
            {
                default:
                case LoadMode.Add:
                    operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                    break;
                case LoadMode.CheckAdd:
                    if (!SceneManager.GetSceneByName(scene_name).isLoaded)
                        operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                    // TODO: else throw an exception (should it?)
                    break;
                case LoadMode.UnloadAdd:
                    SceneManager.UnloadScene(scene_name);
                    operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                    break;
                case LoadMode.ReplaceActive:
                    scene_originally_active_name = SceneManager.GetActiveScene().name;
                    operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                    break;
            }
            yield return operation;
            switch (mode)
            {
                default:
                    break;
                case LoadMode.ReplaceActive:
                    var scene_loaded = SceneManager.GetSceneByName(scene_name);
                    SceneManager.SetActiveScene(scene_loaded);
                    SceneManager.UnloadScene(scene_originally_active_name);
                    break;
            }
        }
    }
}