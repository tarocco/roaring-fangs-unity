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

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace RoaringFangs.Editor
{
    [InitializeOnLoad]
    public static class AnimationHelper
    {
        private const string FTFY = " ==> ";

        #region Preferences

        private const string EnableStickyPropertiesKey = "RoaringFangs.Editor.AnimationHelper.EnableStickyProperties";
        private static bool? _EnableStickyProperties;

        public static bool EnableStickyProperties
        {
            get
            {
                if (!_EnableStickyProperties.HasValue)
                    _EnableStickyProperties = EditorPrefs.GetBool(EnableStickyPropertiesKey, false);
                return _EnableStickyProperties.Value;
            }
            set
            {
                if (value != _EnableStickyProperties)
                {
                    EditorPrefs.SetBool(EnableStickyPropertiesKey, value);
                    _EnableStickyProperties = value;
                }
            }
        }

        private const string SnapBooleanCurveValuesKey = "RoaringFangs.Editor.AnimationHelper.SnapBooleanCurveValues";
        private static bool? _SnapBooleanCurveValues;

        public static bool SnapBooleanCurveValues
        {
            get
            {
                if (!_SnapBooleanCurveValues.HasValue)
                    _SnapBooleanCurveValues = EditorPrefs.GetBool(SnapBooleanCurveValuesKey, false);
                return _SnapBooleanCurveValues.Value;
            }
            set
            {
                if (value != _SnapBooleanCurveValues)
                {
                    EditorPrefs.SetBool(SnapBooleanCurveValuesKey, value);
                    _SnapBooleanCurveValues = value;
                }
            }
        }

        #endregion Preferences

        private static readonly Regex Slashes = new Regex("/+");
        private static readonly string SlashesReplacement = "/";
        private static readonly Regex Parent = new Regex("\\/*[^\\/]*\\/\\.\\.");

        private static string FixPath(string path)
        {
            path = Slashes.Replace(path, SlashesReplacement);
            path = Parent.Replace(path, string.Empty);
            return path;
        }

        /// <summary>
        /// Fixes this shit: https://feedback.unity3d.com/suggestions/allow-retargeting-specific-animation-properties
        /// TODO: make this event-driven like <see cref="EditorApplication.hierarchyWindowChanged"/>
        /// </summary>
        private static void HandleUpdate()
        {
            try
            {
                AnimationClip clip = AnimationWindowUtils.GetCurrentActiveAnimationClip();
                if (clip != null)
                    RepairAnimationClipProperties(clip);
            }
            catch (NullReferenceException ex)
            {
                // Suppress until I figure out a way to not spam users (esp. animators)
            }
        }

        private static void RepairAnimationClipProperties(AnimationClip clip)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);
            for (int i = 0; i < bindings.Length; i++)
            {
                EditorCurveBinding binding = bindings[i];
                string path_fixed = FixPath(binding.path);
                if (path_fixed != binding.path)
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                    AnimationUtility.SetEditorCurve(clip, binding, null);
                    binding.path = path_fixed;
                    AnimationUtility.SetEditorCurve(clip, binding, curve);
                    Debug.Log("Repaired property path: " + binding.path + FTFY + path_fixed);
                }
                if (SnapBooleanCurveValues)
                {
                    // Round IsActive property value to 0/1, or -1/1 if using symmetric booleans
                    if (binding.propertyName == "m_IsActive")
                    {
                        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                        Keyframe[] keys = curve.keys;
                        for (int j = 0; j < curve.length; j++)
                        {
                            Keyframe k = keys[j];
                            k.value = k.value >= 0.5f ? 1f : 0f;
                            keys[j] = k;
                        }
                        curve.keys = keys;
                        AnimationUtility.SetEditorCurve(clip, binding, curve);
                    }
                }
            }
        }

        private static void HandleHierarchyObjectPathChanged(object sender, EditorHelper.HierarchyObjectPathChangedEventArgs args)
        {
            if (EnableStickyProperties)
            {
                object window = AnimationWindowUtils.GetAnimationWindow();
                // If the window is not null (i.e. if it has been made visible at least once)
                if (window != null)
                {
                    object editor = AnimationWindowUtils.GetAnimationEditor(window);
                    object state = AnimationWindowUtils.GetAnimationWindowState(editor);
                    AnimationClip clip = AnimationWindowUtils.GetActiveAnimationClip(state);
                    GameObject root = AnimationWindowUtils.GetRootGameObject(state);
                    if (clip != null && root != null)
                    {
                        string root_path = AnimationUtility.CalculateTransformPath(root.transform, null);
                        int trim_old_path = Math.Min(args.OldPath.Length, root_path.Length + 1);
                        int trim_new_path = Math.Min(args.NewPath.Length, root_path.Length + 1);
                        string old_path_local = args.OldPath.Substring(trim_old_path);
                        string new_path_local = args.NewPath.Substring(trim_new_path);
                        var bindings = AnimationUtility.GetCurveBindings(clip);
                        for (int i = 0; i < bindings.Length; i++)
                        {
                            EditorCurveBinding binding = bindings[i];
                            if (binding.path == old_path_local)
                            {
                                // Remove curve from original binding (un-bind) and add updated binding (re-bind)
                                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                                AnimationUtility.SetEditorCurve(clip, binding, null);
                                binding.path = new_path_local;
                                AnimationUtility.SetEditorCurve(clip, binding, curve);
                                EditorApplication.RepaintAnimationWindow();
                                Debug.Log("Updated property path: " + old_path_local + FTFY + new_path_local);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static string NoClone(string name)
        {
            return name.Substring(0, name.Length - 7);
        }

        /// <summary>
        /// Forces an animation clip to have a defined length of at least <paramref name="length"/>
        /// </summary>
        /// <param name="clip">The animation clip</param>
        /// <param name="length">Minimum length to make the animation clip</param>
        public static void AdjustAnimationClipLength(
            AnimationClip clip,
            float length)
        {
            var dummy_key = new ObjectReferenceKeyframe()
            {
                time = length
            };
            var dummy_array = new[] { dummy_key };
            var binding = EditorCurveBinding.PPtrCurve("Length Adjuster", typeof(UnityObject), "");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, dummy_array);
        }

        /// <summary>
        /// Creates new animation clips with boolean IsActive curves and float
        /// motion curves separated, and a static version of the latter from
        /// the first frame of the animation.
        /// </summary>
        /// <param name="clip">
        /// Animation clip to create partial animations for
        /// </param>
        /// <returns>
        /// Motion, frame-by-frame and single-frame (preamble) animation clips
        /// </returns>
        public static AnimationClip[] CreatePartialAnimationClips(AnimationClip clip)
        {
            AnimationClip motion_clip, fbf_clip, preamble_motion_clip;
            motion_clip = AnimationClip.Instantiate(clip);
            fbf_clip = AnimationClip.Instantiate(clip);
            preamble_motion_clip = AnimationClip.Instantiate(clip);

            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

            motion_clip.name =
                NoClone(motion_clip.name) + " (Motion)";
            fbf_clip.name =
                NoClone(fbf_clip.name) + " (FBF)";
            preamble_motion_clip.name =
                NoClone(preamble_motion_clip.name) + " (Preamble Motion)";

            for (int i = 0; i < bindings.Length; i++)
            {
                var binding = bindings[i];
                if (binding.propertyName == "m_IsActive")
                {
                    AnimationUtility.SetEditorCurve(motion_clip, binding, null);
                    AnimationUtility.SetEditorCurve(preamble_motion_clip, binding, null);
                }
                else
                {
                    AnimationUtility.SetEditorCurve(fbf_clip, binding, null);
                    var preamble_motion_curve = AnimationUtility.GetEditorCurve(
                    preamble_motion_clip,
                    binding);
                    var first_key = preamble_motion_curve.keys.First();
                    preamble_motion_curve.keys = new[] { first_key };
                    // Remark: SetEditorCurve strangely does not like Quaternion curves...
                    AnimationUtility.SetEditorCurve(
                        preamble_motion_clip,
                        binding,
                        preamble_motion_curve);
                }
            }

            AdjustAnimationClipLength(motion_clip, clip.length);
            AdjustAnimationClipLength(fbf_clip, clip.length);
            AdjustAnimationClipLength(preamble_motion_clip, clip.length);

            return new[] { motion_clip, fbf_clip, preamble_motion_clip };
        }

        /// <summary>
        /// Overwrites the contents of the asset at <paramref name="path"/> if
        /// it exists, or else it creates a new asset.
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="asset">The asset</param>
        /// <param name="path">Asset path to read/write</param>
        /// <returns>The existing asset if it exists, or else null</returns>
        private static T CreateOrReplaceAsset<T>(T asset, string path)
            where T : UnityObject
        {
            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (existingAsset == null)
            {
                AssetDatabase.CreateAsset(asset, path);
                existingAsset = asset;
            }
            else
            {
                EditorUtility.CopySerialized(asset, existingAsset);
            }

            return existingAsset;
        }

        [MenuItem("Assets/Roaring Fangs/Create Partial Animations")]
        private static void CreatePartialAnimationsMenuAction()
        {
            var clips = Selection.objects
                .Where(o => o is AnimationClip)
                .Cast<AnimationClip>()
                .ToArray();
            foreach (var clip in clips)
            {
                var clip_path = AssetDatabase.GetAssetPath(clip);
                var clip_directory = Path.GetDirectoryName(clip_path);
                var partial_clips_directory = Path.Combine(
                    clip_directory,
                    "Partial Animations");
                var partial_clips = CreatePartialAnimationClips(clip);
                foreach (var partial_clip in partial_clips)
                {
                    var partial_clip_path = Path.Combine(
                        partial_clips_directory,
                        partial_clip.name + ".anim");
                    CreateOrReplaceAsset(partial_clip, partial_clip_path);
                }
            }
            AssetDatabase.SaveAssets();
        }

        static AnimationHelper()
        {
            EditorApplication.update += HandleUpdate;
            EditorHelper.HierarchyObjectPathChanged += HandleHierarchyObjectPathChanged;
        }
    }
}

#endif