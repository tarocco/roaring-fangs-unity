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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoaringFangs
{
    public static class Scenes
    {
        public static IEnumerable LoadTogether(IEnumerable<string> scene_names, bool skip_if_already_loaded = false)
        {
            var operations_remaining = new List<AsyncOperation>();
            // Asynchronously load the scenes
            foreach (var scene_name in scene_names)
            {
                var scene = SceneManager.GetSceneByName(scene_name);
                if (skip_if_already_loaded && scene.isLoaded)
                    continue;
                Debug.Log("Async loading scene \"" + scene_name + "\"");
                var load = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
                operations_remaining.Add(load);
            }
            // Wait for all of the scenes in the list to be loaded
            while (operations_remaining.Count > 0)
            {
                if (operations_remaining[0].isDone)
                    operations_remaining.RemoveAt(0);
                else
                    yield return new WaitForEndOfFrame();
            }
        }

        public static IEnumerable UnloadTogether(IEnumerable<string> scene_names)
        {
            var operations_remaining = new List<AsyncOperation>();
            // Asynchronously unload the scenes
            foreach (var scene_name in scene_names)
            {
                Debug.Log("Async unloading scene \"" + scene_name + "\"");
                var unload = SceneManager.UnloadSceneAsync(scene_name);
                operations_remaining.Add(unload);
            }
            // Wait for all of the scenes in the list to be unloaded
            while (operations_remaining.Count > 0)
            {
                if (operations_remaining[0].isDone)
                    operations_remaining.RemoveAt(0);
                else
                    yield return new WaitForEndOfFrame();
            }
        }
    }
}