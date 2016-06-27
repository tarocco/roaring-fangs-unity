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

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

using RoaringFangs.Utility;

namespace RoaringFangs.Editor
{
    [InitializeOnLoad]
    public static class EditorHelper
    {
        // TODO: use time, not instruction count
        /// <summary>
        /// Number of parallel operations to perform per update
        /// </summary>
        private const int ParallelTaskOperationsPerUpdate = 200;
        /// <summary>
        /// Threshold of parallel operations to perform continuously
        /// across update calls before presenting a progress indicator
        /// </summary>
        private const int ParallelTaskOperationShowDialog = 1000;
        private static int ParallelCounter = 0;

        private static IEnumerator HierarchyChangedTask;
        private static IEnumerator _ParallelTasksForever;
        private static IEnumerator ParallelTasksForever
        {
            get
            {
                if(_ParallelTasksForever == null)
                    _ParallelTasksForever = GetParallelTasksForever().GetEnumerator();
                return _ParallelTasksForever;
            }
            set
            {
                _ParallelTasksForever = value;
            }
        }

        private static IEnumerable GetParallelTasksForever()
        {
            for (;;)
            {
                object yield_value = null;
                if (HierarchyChangedTask != null && HierarchyChangedTask.MoveNext())
                    yield_value = true;

                yield return yield_value;
            }
        }
        private static IEnumerable<GameObject> GetEverythingInHierarchy()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var root_objects = scene.GetRootGameObjects();
            foreach (GameObject @object in root_objects)
            {
                yield return @object;
                var descendants = TransformUtils.GetAllDescendants(@object.transform);
                foreach (Transform t in descendants)
                {
                    if(t != null)
                        yield return t.gameObject;
                }
            }
        }
        private struct WeakReferenceAndPath
        {
            public WeakReference WeakReference;
            public string Path;
        }
        private static Dictionary<int, WeakReferenceAndPath> HierarchyPaths;
        private static IEnumerable DoHierarchyWindowChanged()
        {
            var hierarchy_paths = new Dictionary<int, WeakReferenceAndPath>();
            var everything = GetEverythingInHierarchy();
            foreach (var @object in everything)
            {
                string path = AnimationUtility.CalculateTransformPath(@object.transform, null);
                var value = new WeakReferenceAndPath() { WeakReference = new WeakReference(@object), Path = path };
                hierarchy_paths[@object.GetHashCode()] = value;
                yield return true;
            }
            if (HierarchyPaths != null)
            {
                foreach (var h in HierarchyPaths)
                {
                    WeakReferenceAndPath wp;
                    bool still_exists = hierarchy_paths.TryGetValue(h.Key, out wp);
                    // if removed
                    if (!still_exists)
                    {
                        GameObject @object = (GameObject)h.Value.WeakReference.Target;
                        OnHierarchyObjectPathChange(@object, h.Value.Path ?? "", wp.Path ?? "");
                        yield return true;
                    }
                }
                foreach (var h in hierarchy_paths)
                {
                    WeakReferenceAndPath wp;
                    bool existed_before = HierarchyPaths.TryGetValue(h.Key, out wp);
                    // if created or changed
                    if (!existed_before || wp.Path != h.Value.Path)
                    {
                        GameObject @object = (GameObject)h.Value.WeakReference.Target;
                        OnHierarchyObjectPathChange(@object, wp.Path ?? "", h.Value.Path ?? "");
                        yield return true;
                    }
                }
            }
            HierarchyPaths = hierarchy_paths;
            yield return null;
        }

        public class HierarchyObjectPathChangedEventArgs : EventArgs
        {
            public readonly GameObject GameObject;
            public readonly string OldPath;
            public readonly string NewPath;
            public HierarchyObjectPathChangedEventArgs(GameObject game_object, string old_path, string new_path) :
                base()
            {
                GameObject = game_object;
                OldPath = old_path;
                NewPath = new_path;
            }
        }
        public delegate void HierarchyObjectPathChangedHandler(object sender, HierarchyObjectPathChangedEventArgs args);
        public static event HierarchyObjectPathChangedHandler HierarchyObjectPathChanged;
        private static void OnHierarchyObjectPathChange(GameObject game_object, string old_path, string new_path)
        {
            if (HierarchyObjectPathChanged != null)
                HierarchyObjectPathChanged(null, new HierarchyObjectPathChangedEventArgs(game_object, old_path, new_path));
        }
        private static void HandleUpdate()
        {
            // Return early if playing and no existing tasks remain
            // Queued tasks will be ignored while playing
            if (EditorApplication.isPlayingOrWillChangePlaymode && ParallelCounter == 0)
                return;
            // Unwrap the parallel tasks loop
            // This is essentially a coroutine
            // TODO: move batch loop into each task's enumerator for finer control over batch size on a task-type basis
            if (ParallelTasksForever != null)
            {
                for (int i = 0; i < ParallelTaskOperationsPerUpdate; i++)
                {
                    try
                    {
                        ParallelTasksForever.MoveNext();
                    }
                    catch(Exception ex)
                    {
                        ParallelTasksForever = null;
                        string message = "Exception thrown in parallel tasks. Aborting.";
                        Debug.LogError(message);
                        throw new InvalidOperationException(message, ex);
                    }
                    if (ParallelTasksForever.Current != null)
                    {
                        if (ParallelCounter > ParallelTaskOperationShowDialog)
                            EditorUtility.DisplayProgressBar("Busy", "Parallel tasks are being performed", 1f);
                        ParallelCounter++;
                    }
                    else
                    {
                        EditorUtility.ClearProgressBar();
                        ParallelCounter = 0;
                        break;
                    }
                }
            }
        }
        private static void HandleHierarchyWindowChanged()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            // Restart the DoHierarchyWindowChanged task
            HierarchyChangedTask = DoHierarchyWindowChanged().GetEnumerator();
        }
        static EditorHelper()
        {
            EditorApplication.update += HandleUpdate;
            EditorApplication.hierarchyWindowChanged += HandleHierarchyWindowChanged;
            // Initialize by calling the handler once at the start
            HandleHierarchyWindowChanged();
        }
    }
}

#endif