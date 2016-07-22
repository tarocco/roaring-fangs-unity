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

#if PSD_IMPORTER_MODULE && SPRITES_AND_BONES

using PhotoshopFile;
using RoaringFangs.Utility;
using SpritesAndBones;
using subjectnerdagreement.psdexport;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.Animation.Editor
{
    public class AnimationConstructor : SpriteConstructor, IPsdConstructorDependent
    {
        private ILookup<string, PenToolPathResource> _PenToolPathResources;

        private struct IKWithMetadata
        {
            public InverseKinematics IK;
            public IKMetadata Metadata;
        }

        private List<IKWithMetadata> _IKData;
        private Dictionary<string, Transform> _IKDestinations;

        /// <summary>
        /// Extracts JSON instances from a string (assumes valid JSON)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static IEnumerable<string> ExtractJSONStrings(string text)
        {
            int depth = 0;
            int match_start_index = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                switch (c)
                {
                    case '{':
                        if (depth == 0)
                            match_start_index = i;
                        depth++;
                        break;

                    case '}':
                        depth--;
                        if (depth == 0)
                            yield return text.Substring(match_start_index, i - match_start_index + 1);
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Returns the text before any JSON onset
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string TextBeforeJSON(string text)
        {
            int idx_first_open_brace = text.IndexOf('{');
            if (idx_first_open_brace != -1)
                return text.Substring(0, idx_first_open_brace).TrimEnd(' ');
            else
                return text;
        }

        private class InlineMetadata
        {
            public IKMetadata IK;

            public override string ToString()
            {
                return "< IK: " + IK.ToString() + ">";
            }
        }

        private class IKMetadata
        {
            public string Parent;
            public int ChainLength = 1;

            public override string ToString()
            {
                return "< Parent: " + Parent + ", ChainLength: " + ChainLength + ">";
            }
        }

        private InlineMetadata GetFirstMetadata(string text)
        {
            IEnumerable<string> jsons = ExtractJSONStrings(text);
            if (jsons.Any())
            {
                var reader = new JsonFx.Json.JsonReader();
                return reader.Read<InlineMetadata>(jsons.First());
            }
            else
                return null;
        }

        private BuildPSDArgs _Context;

        public BuildPSDArgs Context
        {
            get { return _Context; }
            set { _Context = value; }
        }

        public override string MenuName
        {
            get { return "Animation Constructor"; }
        }

        public override void AddComponents(
            int layerIndex,
            GameObject imageObject,
            Sprite sprite,
            TextureImporterSettings texture_importer_settings)
        {
            base.AddComponents(layerIndex, imageObject, sprite, texture_importer_settings);
        }

        public override GameObject CreateGameObject(string name, GameObject parent)
        {
            return base.CreateGameObject(name, parent);
        }

        public override Matrix4x4 GetGroupMatrix(Matrix4x4 rootM__worldToLocal, GameObject groupRoot, SpriteAlignment alignment)
        {
            var settings = Context.Settings;

            var size = new Vector2(settings.Psd.ColumnCount, settings.Psd.RowCount);
            //var scale = PsdExportSettings.GetScaleValues(settings.ScaleBy).Scale;

            var matching_paths = _PenToolPathResources[groupRoot.name];

            if (matching_paths.Any())
            {
                var path = matching_paths.First();
                //Debug.Log("Found a path for group \"" + group_info.name + "\"");
                // Override group position with the position of the first path knot
                var path_knots = path.SubpathRecords
                    .Where(r => r is PenToolPathResource.SubpathBezierKnotRecord)
                    .Cast<PenToolPathResource.SubpathBezierKnotRecord>();
                var first_knot = path_knots.First();
                var second_knot = path_knots.Skip(1).First();
                var point_anchor = first_knot.Anchor;
                var point_fwd = second_knot.Anchor;
                var position_normalized = new Vector2(point_anchor.X, -point_anchor.Y);
                var direction_normalized = new Vector2(point_fwd.X, -point_fwd.Y) - position_normalized;
                Vector3 position = position_normalized / settings.PixelsToUnitSize;
                position.Scale(size);
                Vector3 direction = direction_normalized / settings.PixelsToUnitSize;
                direction.Scale(size);
                // TODO: adjust this in case bone axis option is re-implemented
                Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction_normalized);
                Matrix4x4 groupM = Matrix4x4.TRS(position, rotation, Vector3.one);
                return groupM;
            }
            else
                return base.GetGroupMatrix(rootM__worldToLocal, groupRoot, alignment);
        }

        public override Matrix4x4 GetLayerMatrix(Rect layerSize, Vector2 layerPivot, float pixelsToUnitSize)
        {
            return base.GetLayerMatrix(layerSize, layerPivot, pixelsToUnitSize);
        }

        public override void HandleGroupClose(GameObject groupParent)
        {
            GameObject root = Context.Root;
            Transform root_transform;
            if (root)
                root_transform = root.transform;
            else
                root_transform = null;
            var group_transform = groupParent.transform;
            var group_transform_parent = group_transform.parent;
            // If closing the last group (finished)
            if (group_transform_parent == root_transform)
            {
                HandleFinish();
            }
        }

        public override void HandleGroupOpen(GameObject groupParent)
        {
            GameObject root = Context.Root;
            Transform root_transform;
            Matrix4x4 root_transform_m_local, root_transform_m_world;
            if (root)
            {
                root_transform = root.transform;
                root_transform_m_local = root_transform.worldToLocalMatrix;
                root_transform_m_world = root_transform.localToWorldMatrix;
            }
            else
            {
                root_transform = null;
                root_transform_m_local = Matrix4x4.identity;
                root_transform_m_world = Matrix4x4.identity;
            }

            var group_transform = groupParent.transform;
            var group_transform_parent = group_transform.parent;
            // If opening the first group (beginning)
            if (group_transform_parent == root_transform)
            {
                HandleBegin();
            }

            var settings = Context.Settings;
            var size = new Vector2(settings.Psd.ColumnCount, settings.Psd.RowCount);

            //var scale = PsdExportSettings.GetScaleValues(settings.ScaleBy).Scale;

            // Store the full groupParent name for metadata parsing
            var groupParent_name_orig = groupParent.name;

            // Mutate to the "plain" groupParent name as everything before the first JSON
            groupParent.name = TextBeforeJSON(groupParent.name);

            var matching_paths = _PenToolPathResources[groupParent.name];
            if (matching_paths.Any())
            {
                var path = matching_paths.First();
                var path_knots = path.SubpathRecords
                    .Where(r => r is PenToolPathResource.SubpathBezierKnotRecord)
                    .Cast<PenToolPathResource.SubpathBezierKnotRecord>();
                var first_knot = path_knots.First();
                var second_knot = path_knots.Skip(1).First();
                var point_anchor = first_knot.Anchor;
                var point_fwd = second_knot.Anchor;
                var position_normalized = new Vector2(point_anchor.X, -point_anchor.Y);
                var direction_normalized = new Vector2(point_fwd.X, -point_fwd.Y) - position_normalized;
                Vector3 position = position_normalized / settings.PixelsToUnitSize;
                position.Scale(size);
                Vector3 direction = direction_normalized / settings.PixelsToUnitSize;
                direction.Scale(size);
                Bone bone = groupParent.AddComponent<Bone>();
                // TODO: restore this in case bone axis option is re-implemented
                // bone.boneAxis = Bone.BoneAxis.Y;
                //bone.Direction = direction;
                bone.length = direction.magnitude;

                // Read metadata by parsing the JSON from the original group name
                var metadata = GetFirstMetadata(groupParent_name_orig);
                if (metadata != null)
                {
                    var ik_metadata = metadata.IK;
                    if (ik_metadata != null)
                    {
                        var ik = groupParent.AddComponent<InverseKinematics>();
                        ik.chainLength = ik_metadata.ChainLength;
                        var target = new GameObject(groupParent.name + " IK");
                        var helper = target.AddComponent<Helper>();
                        var target_transform = target.transform;

                        // Bone (group) position and rotation have not been updated yet,
                        // so we will need to get the group matrix using GetGroupMatrix()
                        var groupParent_m = GetGroupMatrix(root_transform_m_local, groupParent, settings.Pivot);
                        var groupParent_m_world = root_transform_m_world * groupParent_m;
                        var groupParent_position = TRS.GetPosition(ref groupParent_m_world);
                        var groupParent_rotation = TRS.GetRotation(ref groupParent_m_world);
                        var target_position_offset = groupParent_rotation * new Vector3(0f, bone.length, 0f);

                        target_transform.rotation = groupParent_rotation;
                        target_transform.position = groupParent_position + target_position_offset;
                        target_transform.SetParent(root_transform, true);

                        ik.target = target_transform;
                        _IKData.Add(new IKWithMetadata()
                        {
                            IK = ik,
                            Metadata = ik_metadata
                        });
                        _IKDestinations[target.name] = target.transform;
                    }
                }
                _IKDestinations[groupParent.name] = groupParent.transform;
            }
        }

        protected void HandleBegin()
        {
            var settings = Context.Settings;
            var psd = settings.Psd;
            var image_resources = psd.ImageResources;
            _PenToolPathResources = _PenToolPathResources ?? image_resources
                .Where(r => r is PenToolPathResource)
                .Cast<PenToolPathResource>()
                .ToLookup(p => p.Name);
            _IKData = new List<IKWithMetadata>();
            _IKDestinations = new Dictionary<string, Transform>();
        }

        protected void HandleFinish()
        {
            GameObject root = Context.Root;
            Transform root_transform;
            if (root)
                root_transform = root.transform;
            else
                root_transform = null;

            foreach (var e in _IKData)
            {
                var ik = e.IK;
                var metadata = e.Metadata;
                var destination_name = metadata.Parent;
                var ik_target = ik.target;
                Transform destination;
                if (!String.IsNullOrEmpty(destination_name) && _IKDestinations.TryGetValue(destination_name, out destination))
                {
                    ik_target.SetParent(destination, true);
                }
                else if (root_transform != null)
                {
                    ik_target.SetParent(root_transform, true);
                }
            }
        }
    }
}

#endif