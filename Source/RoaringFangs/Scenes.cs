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
using RoaringFangs.SceneManagement;

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
            SetActive,
            ReplaceActive,
            MergeWithActive,
            Editor = 0xFFFF,
        }

        public class CoroutineProcessor : MonoBehaviour
        {
            // Dummy MonoBahaviour
        }

        public struct SceneInfo
        {
            public readonly Scene Scene;
            public readonly LoadSceneMode LoadSceneMode;

            public SceneInfo(Scene scene, LoadSceneMode mode)
            {
                Scene = scene;
                LoadSceneMode = mode;
            }
        }

        private static List<string> _ScenesLoading = new List<string>();

        public static IEnumerable<string> ScenesLoading
        {
            get { return _ScenesLoading; }
        }

        private static List<SceneInfo> _ScenesLoaded = new List<SceneInfo>();

        public static IEnumerable<SceneInfo> ScenesLoaded
        {
            get { return _ScenesLoaded; }
        }

        static Scenes()
        {
            var scene_count = SceneManager.sceneCount;
            for (int i = 0; i < scene_count; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                var scene_info = new SceneInfo(scene, LoadSceneMode.Single);
                _ScenesLoaded.Add(scene_info);
            }
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            var active_scene = SceneManager.GetActiveScene();
            _ScenesLoaded.Add(new SceneInfo(active_scene, LoadSceneMode.Single));
        }

        private static void HandleSceneUnloaded(Scene scene)
        {
            _ScenesLoading.RemoveAll(s => s == scene.name); // Just in case?
            _ScenesLoaded.RemoveAll(i => i.Scene == scene);
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _ScenesLoading.RemoveAll(s => s == scene.name);
            _ScenesLoaded.Add(new SceneInfo(scene, mode));
        }

        private static bool IsSceneLoading(string scene_name)
        {
            return _ScenesLoading.Any(s => s == scene_name);
        }

        private static bool IsSceneLoaded(string scene_name)
        {
            return _ScenesLoaded.Any(i => i.Scene.name == scene_name);
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
                    _ScenesLoading.Add(scene_name);
                    break;

                case LoadMode.CheckAdd:
                    if (!IsSceneLoadingOrLoaded(scene_name))
                    {
                        operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                        _ScenesLoading.Add(scene_name);
                    }
                    // TODO: else throw an exception (should it?)
                    break;

                case LoadMode.UnloadAdd:
                    SceneManager.UnloadScene(scene_name);
                    operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                    _ScenesLoading.Add(scene_name);
                    break;

                case LoadMode.SetActive:
                    if (!IsSceneLoadingOrLoaded(scene_name))
                    {
                        operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                        _ScenesLoading.Add(scene_name);
                    }
                    break;

                case LoadMode.ReplaceActive:
                    if (!IsSceneLoadingOrLoaded(scene_name))
                    {
                        scene_originally_active_name = SceneManager.GetActiveScene().name;
                        SceneManager.UnloadScene(scene_originally_active_name);
                        operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                        _ScenesLoading.Add(scene_name);
                    }
                    break;

                case LoadMode.MergeWithActive:
                    scene_loaded = SceneManager.GetSceneByName(scene_name);
                    if (!IsSceneLoadingOrLoaded(scene_name))
                    {
                        operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                        _ScenesLoading.Add(scene_name);
                    }
                    break;
            }
            yield return operation;
            _ScenesLoading.RemoveAll(s => s == scene_name);
            switch (mode)
            {
                default:
                    break;

                case LoadMode.SetActive:
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

        public static IEnumerable LoadAsync(object self, string scene_name, LoadMode mode, SceneLoadCompleteHandler callback)
        {
            Scene active = SceneManager.GetActiveScene();
            var operations = LoadAsync(scene_name, mode);
            foreach (var y in operations)
                yield return y;
            Scene loaded = SceneManager.GetSceneByName(scene_name);
            if (callback != null)
                callback(self, new SceneLoadCompleteEventArgs(active, loaded, mode));
        }

        public static void StartLoadAsync(MonoBehaviour self, string scene_name, LoadMode mode, SceneLoadCompleteHandler callback = null)
        {
            self.StartCoroutine(LoadAsync(self, scene_name, mode, callback).GetEnumerator());
        }

        public static void StartLoadMany(MonoBehaviour self, LoadMode mode, SceneLoadManyCompletedHandler callback, params string[] scene_names)
        {
            var scene_names_list = new List<string>(scene_names);
            var scenes_loaded_args = new List<SceneLoadCompleteEventArgs>();
            SceneLoadCompleteHandler scene_load_callback = (object sender, SceneLoadCompleteEventArgs args) =>
            {
                scene_names_list.Remove(args.LoadedScene.name);
                scenes_loaded_args.Add(args);
                if (scene_names_list.Count == 0)
                {
                    if (callback != null)
                    {
                        var collection = scenes_loaded_args.ToArray();
                        var args_many = new SceneLoadManyCompleteEventArgs(collection);
                        callback(self, args_many);
                    }
                }
            };
            foreach (string scene_name in scene_names)
                StartLoadAsync(self, scene_name, mode, scene_load_callback);
        }

        public static void StartUnloadMany(MonoBehaviour self, SceneUnloadManyCompletedHandler callback, params string[] scene_names)
        {
            var scene_names_list = new List<string>(scene_names);
            var scenes_unloaded_args = new List<SceneUnloadCompleteEventArgs>();
            SceneUnloadCompleteHandler scene_unload_callback = (object sender, SceneUnloadCompleteEventArgs args) =>
            {
                scene_names_list.Remove(args.UnloadedSceneName);
                scenes_unloaded_args.Add(args);
                if (scene_names_list.Count == 0)
                {
                    if (callback != null)
                    {
                        var collection = scenes_unloaded_args.ToArray();
                        var args_many = new SceneUnloadManyCompleteEventArgs(collection);
                        callback(self, args_many);
                    }
                }
            };
            foreach (string scene_name in scene_names)
                StartUnloadAsync(self, scene_name, scene_unload_callback);
        }

        public static IEnumerable UnloadAsync(object self, string scene_name, SceneUnloadCompleteHandler callback)
        {
            yield return new WaitForEndOfFrame();
            SceneManager.UnloadScene(scene_name);
            if (callback != null)
                callback(self, new SceneUnloadCompleteEventArgs(scene_name));
        }

        public static void StartUnloadAsync(MonoBehaviour self, string scene_name, SceneUnloadCompleteHandler callback)
        {
            self.StartCoroutine(UnloadAsync(self, scene_name, callback).GetEnumerator());
        }
    }
}