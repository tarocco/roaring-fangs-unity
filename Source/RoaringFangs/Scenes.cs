﻿/*
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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoaringFangs
{
    /// <summary>
    /// Use this as a COMPLETE replacement for <see cref="UnityEngine.Scenemanagement.SceneManager"/>
    /// </summary>
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

        public class CoroutineProcessor : MonoBehaviour
        {
            // Dummy MonoBahaviour
        }

        private struct SceneInfo
        {
            public readonly Scene Scene;
            public readonly LoadSceneMode LoadSceneMode;

            public SceneInfo(Scene scene, LoadSceneMode mode)
            {
                Scene = scene;
                LoadSceneMode = mode;
            }
        }

        private static List<string> ScenesLoading = new List<string>();

        private static List<SceneInfo> ScenesLoaded = new List<SceneInfo>();

        static Scenes()
        {
            var scene_count = SceneManager.sceneCount;
            for (int i = 0; i < scene_count; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                var scene_info = new SceneInfo(scene, LoadSceneMode.Single);
                ScenesLoaded.Add(scene_info);
            }
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            var active_scene = SceneManager.GetActiveScene();
            ScenesLoaded.Add(new SceneInfo(active_scene, LoadSceneMode.Single));
        }

        private static void HandleSceneUnloaded(Scene scene)
        {
            ScenesLoading.RemoveAll(s => s == scene.name); // Just in case?
            ScenesLoaded.RemoveAll(i => i.Scene == scene);
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ScenesLoading.RemoveAll(s => s == scene.name);
            ScenesLoaded.Add(new SceneInfo(scene, mode));
        }

        private static bool IsSceneLoading(string scene_name)
        {
            return ScenesLoading.Any(s => s == scene_name);
        }

        private static bool IsSceneLoaded(string scene_name)
        {
            return ScenesLoaded.Any(i => i.Scene.name == scene_name);
        }

        private static bool IsSceneLoadingOrLoaded(string scene_name)
        {
            return IsSceneLoading(scene_name) || IsSceneLoaded(scene_name);
        }

        public static IEnumerable LoadAsync(string scene_name, LoadMode mode)
        {
            yield return new WaitForEndOfFrame();
            // This is here because of a Mono bug...
            YieldInstruction operation = null;
            string scene_originally_active_name = null;
            Scene scene_loaded;
            switch (mode)
            {
                default:
                case LoadMode.Add:
                    operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                    ScenesLoading.Add(scene_name);
                    break;

                case LoadMode.CheckAdd:
                    if (!IsSceneLoadingOrLoaded(scene_name))
                    {
                        operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                        ScenesLoading.Add(scene_name);
                    }
                    // TODO: else throw an exception (should it?)
                    break;

                case LoadMode.UnloadAdd:
                    SceneManager.UnloadScene(scene_name);
                    operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                    ScenesLoading.Add(scene_name);
                    break;

                case LoadMode.ReplaceActive:
                    if (!IsSceneLoadingOrLoaded(scene_name))
                    {
                        scene_originally_active_name = SceneManager.GetActiveScene().name;
                        SceneManager.UnloadScene(scene_originally_active_name);
                        operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                        ScenesLoading.Add(scene_name);
                    }
                    break;

                case LoadMode.MergeWithActive:
                    scene_loaded = SceneManager.GetSceneByName(scene_name);
                    if (!IsSceneLoadingOrLoaded(scene_name))
                    {
                        operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                        ScenesLoading.Add(scene_name);
                    }
                    break;
            }
            yield return operation;
            ScenesLoading.RemoveAll(s => s == scene_name);
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

        public static IEnumerable LoadAsync(object self, string scene_name, LoadMode mode, SceneLoadCompletedHandler callback)
        {
            Scene active = SceneManager.GetActiveScene();
            var operations = LoadAsync(scene_name, mode);
            foreach (var y in operations)
                yield return y;
            Scene loaded = SceneManager.GetSceneByName(scene_name);
            if (callback != null)
                callback(self, new SceneLoadCompleteEventArgs(active, loaded, mode));
        }

        public static void StartLoadAsync(MonoBehaviour self, string scene_name, LoadMode mode, SceneLoadCompletedHandler callback = null)
        {
            self.StartCoroutine(LoadAsync(self, scene_name, mode, callback).GetEnumerator());
        }

        public static IEnumerable UnloadAsync(object self, string scene_name, SceneUnloadCompletedHandler callback)
        {
            yield return new WaitForEndOfFrame();
            SceneManager.UnloadScene(scene_name);
            if (callback != null)
                callback(self, new SceneUnloadCompleteEventArgs(scene_name));
        }

        public static void StartUnloadAsync(MonoBehaviour self, string scene_name, SceneUnloadCompletedHandler callback)
        {
            self.StartCoroutine(UnloadAsync(self, scene_name, callback).GetEnumerator());
        }
    }
}