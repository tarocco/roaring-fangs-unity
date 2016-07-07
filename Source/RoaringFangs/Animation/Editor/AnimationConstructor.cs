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
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PhotoshopFile;
using subjectnerdagreement.psdexport;

using SpritesAndBones;

namespace RoaringFangs.Animation.Editor
{
    public class AnimationConstructor : SpriteConstructor, IPsdConstructorDependent
    {
        //private Stack<GameObject> _GroupStack = new Stack<GameObject>();
        private ILookup<string, PenToolPathResource> _PenToolPathResources;

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

        public override Matrix4x4 GetGroupMatrix(Matrix4x4 rootBaseM__worldToLocal, GameObject groupRoot, SpriteAlignment alignment)
        {
            var settings = Context.Settings;
            CacheResources();
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
                return base.GetGroupMatrix(rootBaseM__worldToLocal, groupRoot, alignment);
        }

        public override Matrix4x4 GetLayerMatrix(Rect layerSize, Vector2 layerPivot, float pixelsToUnitSize)
        {
            return base.GetLayerMatrix(layerSize, layerPivot, pixelsToUnitSize);
        }

        public override void HandleGroupClose(GameObject groupParent)
        {
            //_GroupStack.Pop();
        }

        public override void HandleGroupOpen(GameObject groupParent)
        {
            CacheResources();
            //_GroupStack.Push(groupParent);

            var settings = Context.Settings;
            var size = new Vector2(settings.Psd.ColumnCount, settings.Psd.RowCount);
            //var scale = PsdExportSettings.GetScaleValues(settings.ScaleBy).Scale;

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
            }
        }

        private void CacheResources()
        {
            var settings = Context.Settings;
            var psd = settings.Psd;
            var image_resources = psd.ImageResources;
            _PenToolPathResources = _PenToolPathResources ?? image_resources
                .Where(r => r is PenToolPathResource)
                .Cast<PenToolPathResource>()
                .ToLookup(p => p.Name);
        }
    }
}
#endif