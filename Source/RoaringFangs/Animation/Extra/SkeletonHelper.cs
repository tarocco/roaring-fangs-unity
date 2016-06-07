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

#if SPRITES_AND_BONES
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RoaringFangs.Utility;

using SpritesAndBones;

namespace RoaringFangs.Animation.Extra
{
    [RequireComponent(typeof(Skeleton))]
    public class SkeletonHelper : MonoBehaviour
    {
        [System.Serializable]
        public struct Relationship
        {
            public Transform Self;
            public Transform Parent;
            public int SiblingIndex;
        }

        public string BonePrefix = "][";

        public Relationship[] BoneRelationships;
        public Relationship[] GroupRelationships;

        public bool OptionOperateOnMatchingBones = true;
        public bool OptionAssimilateAlignGroups = true;
        public IEnumerable<Transform> AllDescendants
        {
            get { return TransformUtils.GetAllDescendants(transform); }
        }
        public IEnumerable<Transform> GetBones(string prefix, bool matching)
        {
            if (matching)
                return TransformUtils.GetMatchingTransforms(transform, prefix);
            else
                return TransformUtils.GetTransforms(GetComponentsInChildren<Bone>());
        }
        public IEnumerable<Transform> GetRootAndBones(string prefix, bool matching)
        {
            yield return transform;
            foreach(Transform t in GetBones(prefix, matching))
                yield return t;
        }
        public IEnumerable<Transform> GetGroups(string prefix, bool matching)
        {
            if (matching)
            {
                // Groups parented to matching bones
                return SkeletonHelper.GetGroupsInChildren(
                    GetRootAndBones(prefix, true));
            }
            else
            {
                // Groups parented to transforms of GameObjects
                // with Bone components, excluding matching bones
                return SkeletonHelper.GetGroupsInChildren(
                    GetRootAndBones(prefix, false),
                    GetRootAndBones(prefix, true));
            }
        }

        public static IEnumerable<Transform> GetGroupsInChildren(IEnumerable<Transform> bone_transforms, IEnumerable<Transform> exclude = null)
        {
            var children = new HashSet<Transform>(TransformUtils.GetAllChildren(bone_transforms));
            foreach (Transform group_transform in bone_transforms)
                children.Remove(group_transform);
            if (exclude != null)
            {
                foreach (Transform t_e in exclude)
                    children.Remove(t_e);
            }
            return children;
        }

        public void AlignTransformsToFirstSpriteDescendant(string prefix, bool matching)
        {
            IEnumerable<Transform> bones = GetBones(prefix, matching);
            HashSet<Transform> exclude = new HashSet<Transform>(GetBones(prefix, true));

            // For each bone
            foreach (Transform bone_transform in bones)
            {
                Transform transform_first_child_group = null;
                // For each child of bone
                foreach (Transform child_of_bone in bone_transform)
                {
                    // If it's not a bone, it's a group
                    if (exclude == null || !exclude.Contains(child_of_bone))
                    {
                        transform_first_child_group = transform_first_child_group ?? child_of_bone.transform;
                        // Look for the first descendant sprite renderer
                        var first_child_sprite = child_of_bone.GetComponentInChildren<SpriteRenderer>();
                        if (first_child_sprite != null)
                        {
                            // Move group transform without affecting children
                            Vector3 offset = first_child_sprite.transform.localPosition;
                            child_of_bone.localPosition += offset;
                            foreach (Transform grandchild in child_of_bone)
                                grandchild.localPosition -= offset;
                            break;
                        }
                    }
                }
                if (transform_first_child_group)
                {
                    Vector3 offset = transform_first_child_group.transform.localPosition;
                    bone_transform.localPosition += offset;
                    foreach (Transform child_of_bone in bone_transform)
                        child_of_bone.localPosition -= offset;
                }
            }
        }

        public static IEnumerable<Transform> Link(
            IEnumerable<Relationship> relationships,
            TransformUtils.SetTransformParentDelegate set_parent = null)
        {
            // Default to SetTransformParent
            if(set_parent == null)
                set_parent = TransformUtils.SetTransformParent;
            foreach (Relationship relationship in relationships)
            {
                set_parent(relationship.Self, relationship.Parent);
                yield return relationship.Self;
            }
            foreach (Relationship relationship in relationships)
            {
                relationship.Self.SetSiblingIndex(relationship.SiblingIndex);
            }
        }

        public static IEnumerable<Relationship> Unlink(
            IEnumerable<Relationship> existing_relationships,
            IEnumerable<Transform> transforms,
            Transform destination,
            TransformUtils.SetTransformParentDelegate set_parent = null)
        {
            // Default to SetTransformParent
            set_parent = set_parent ?? TransformUtils.SetTransformParent;
            var existing_self_set = new HashSet<Transform>();
            if (existing_relationships != null)
            {
                foreach (var relationship in existing_relationships)
                {
                    existing_self_set.Add(relationship.Self);
                    yield return relationship;
                }
            }

            foreach (Transform t in transforms)
            {
                if (!existing_self_set.Contains(t))
                {
                    var relationship = new Relationship()
                    {
                        Self = t,
                        Parent = t.parent,
                        SiblingIndex = t.GetSiblingIndex()
                    };
                    set_parent(t, destination);
                    yield return relationship;
                }
            }
        }
    }
}
#endif
