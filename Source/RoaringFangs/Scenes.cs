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
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoaringFangs
{
    public static class Scenes
    {
        public enum LoadMode
        {
            Add,
            CheckAdd,
            UnloadAdd,
            ReplaceActive,
            MergeWithActive,
        }

        public class SceneLoadCompleteEventArgs : EventArgs
        {
            public readonly Scene PreviousActiveScene;
            public readonly Scene LoadedScene;
            public readonly LoadMode Mode;

            public SceneLoadCompleteEventArgs(Scene previous_active_scene, Scene loaded_scene, Scenes.LoadMode mode)
            {
                PreviousActiveScene = previous_active_scene;
                LoadedScene = loaded_scene;
                Mode = mode;
            }
        }

        public delegate void SceneLoadCompletedHandler(object sender, SceneLoadCompleteEventArgs args);

        public static void Load(string scene_name, LoadMode mode)
        {
            Scene scene_loaded;
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
                    // Uhh...
                    // http://forum.unity3d.com/threads/unity-hangs-on-scenemanager-unloadscene.380116/
                    throw new InvalidOperationException("Cannot replace active scene using blocking method. Use LoadAsync for LoadMode.ReplaceActive.");
                /*
                string scene_originally_active_name = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene(scene_name, LoadSceneMode.Additive);
                scene_loaded = SceneManager.GetSceneByName(scene_name);
                SceneManager.SetActiveScene(scene_loaded);
                SceneManager.UnloadScene(scene_originally_active_name);
                break;
                */
                case LoadMode.MergeWithActive:
                    scene_loaded = SceneManager.GetSceneByName(scene_name);
                    if (!scene_loaded.isLoaded)
                        SceneManager.LoadScene(scene_name, LoadSceneMode.Additive);
                    scene_loaded = SceneManager.GetSceneByName(scene_name);
                    SceneManager.MergeScenes(scene_loaded, SceneManager.GetActiveScene());
                    break;
            }
        }

        public static IEnumerable LoadAsync(string scene_name, LoadMode mode)
        {
            // This is here because of a Mono bug...
            YieldInstruction operation = null;
            string scene_originally_active_name = null;
            Scene scene_loaded;
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
                    SceneManager.UnloadScene(scene_originally_active_name);
                    operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                    break;

                case LoadMode.MergeWithActive:
                    scene_loaded = SceneManager.GetSceneByName(scene_name);
                    if (!scene_loaded.isLoaded)
                        operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                    break;
            }
            yield return operation;
            switch (mode)
            {
                default:
                    break;

                case LoadMode.ReplaceActive:
                    scene_loaded = SceneManager.GetSceneByName(scene_name);
                    SceneManager.SetActiveScene(scene_loaded);
                    break;

                case LoadMode.MergeWithActive:
                    scene_loaded = SceneManager.GetSceneByName(scene_name);
                    SceneManager.MergeScenes(scene_loaded, SceneManager.GetActiveScene());
                    break;
            }
        }

        public static void Load(MonoBehaviour self, string scene_name, LoadMode mode, bool @async, SceneLoadCompletedHandler callback = null)
        {
            if (@async)
                self.StartCoroutine(LoadAsync(self, scene_name, mode, callback).GetEnumerator());
            else
            {
                Load(self, scene_name, mode, callback);
            }
        }

        private static void Load(object self, string scene_name, LoadMode mode, SceneLoadCompletedHandler callback)
        {
            Scene active = SceneManager.GetActiveScene();
            Load(scene_name, mode);
            Scene loaded = SceneManager.GetSceneByName(scene_name);
            if (callback != null)
                callback(self, new SceneLoadCompleteEventArgs(active, loaded, mode));
        }

        private static IEnumerable LoadAsync(object self, string scene_name, LoadMode mode, SceneLoadCompletedHandler callback)
        {
            Scene active = SceneManager.GetActiveScene();
            var operations = LoadAsync(scene_name, mode);
            foreach (var y in operations)
                yield return y;
            Scene loaded = SceneManager.GetSceneByName(scene_name);
            if (callback != null)
                callback(self, new SceneLoadCompleteEventArgs(active, loaded, mode));
        }

        public static void Unload(string scene_name)
        {
            SceneManager.UnloadScene(scene_name);
        }

        public static void UnloadActive()
        {
            SceneManager.UnloadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}