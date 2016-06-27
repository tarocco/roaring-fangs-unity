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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using RoaringFangs.Animation.Extra;
using RoaringFangs.Utility;
using RoaringFangs.Editor;

using SpritesAndBones;

namespace RoaringFangs.Animation.Extra.Editor
{
    [CustomEditor(typeof(SkeletonHelper))]
    [CanEditMultipleObjects]
    public class SkeletonHelperEditor : UnityEditor.Editor
    {
        SkeletonHelper Self;
        GameObject AssimilationObject;

        bool HasBones;
        
        void OnEnable()
        {
            Self = (SkeletonHelper)target;
        }
        public override void OnInspectorGUI()
        {
            HasBones = Self.GetComponentsInChildren<Bone>().Length > 0;

            serializedObject.Update();

            // Draw these at the end
            DrawPropertiesExcluding(
                serializedObject,
                "BoneRelationships",
                "GroupRelationships",
                "OptionOperateOnMatchingBones",
                "OptionAssimilateAlignGroups");

            #region Inspector Setup

            UnityEngine.GUI.color = Color.white;

            bool gui_enabled = UnityEngine.GUI.enabled;

            bool has_bone_rel = Self.BoneRelationships != null && Self.BoneRelationships.Length > 0;
            bool has_group_rel = Self.GroupRelationships != null && Self.GroupRelationships.Length > 0;

            bool has_bones_effectively = HasBones || Self.OptionOperateOnMatchingBones;

            #endregion

            #region Operations
            EditorGUILayout.LabelField("Operations", GUIHelper.StyleLabelHeaderHint);

            #region Create Bones

            EditorGUILayout.BeginHorizontal();
            
            UnityEngine.GUI.color = GUIHelper.Green;
            if (GUILayout.Button("Create Bones"))
            {
                OnCreateBones();
            }
            UnityEngine.GUI.enabled = gui_enabled && HasBones;
            UnityEngine.GUI.color = GUIHelper.Red;
            if (GUILayout.Button("Destroy Bones"))
            {
                OnDestroyBones();
            }
            UnityEngine.GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();

            #endregion

            UnityEngine.GUI.enabled = gui_enabled && has_bones_effectively;

            #region Align Bones & Groups

            if (GUILayout.Button("Align bones and groups to contents", GUIHelper.StyleButtonBold))
            {
                OnAlignTransformsToFirstSpriteDescendant();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Orient Parent Bones"))
            {
                OnOrientParentBones();
            }
            if (GUILayout.Button("Reach Parent Bones"))
            {
                OnReachParentBones();
            }

            EditorGUILayout.EndHorizontal();

            #endregion

            #region Linkage

            #region Bone Linkage

            EditorGUILayout.BeginHorizontal();

            UnityEngine.GUI.enabled = gui_enabled && has_bone_rel;

            if (GUILayout.Button("Link Bones"))
            {
               OnLinkBones();
            }

            UnityEngine.GUI.enabled = gui_enabled && has_bones_effectively;

            if (GUILayout.Button("Unlink Bones"))
            {
                OnUnlinkBones();
            }

            EditorGUILayout.EndHorizontal();

            #endregion

            #region Group Linkage

            EditorGUILayout.BeginHorizontal();

            UnityEngine.GUI.enabled = gui_enabled && has_group_rel;

            if (GUILayout.Button("Link Groups"))
            {
                OnLinkGroups();
            }

            UnityEngine.GUI.enabled = gui_enabled && has_bones_effectively;

            if (GUILayout.Button("Unlink Groups"))
            {
               OnUnlinkGroups();
            }

            UnityEngine.GUI.enabled = gui_enabled;

            EditorGUILayout.EndHorizontal();

            #endregion

            #endregion

            #region Visibility

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Lock Groups"))
            {
                OnLockGroups();
            }
            if (GUILayout.Button("Unlock Groups"))
            {
                OnUnlockGroups();
            }
            
            EditorGUILayout.EndHorizontal();

            #endregion

            Self.OptionOperateOnMatchingBones = EditorGUILayout.ToggleLeft(
               "Operate on Matching Bones and Groups", Self.OptionOperateOnMatchingBones);

            #endregion

            #region Assimilation

            EditorGUILayout.LabelField("Assimilation", GUIHelper.StyleLabelHeaderHint);

            var area_assimilation = EditorGUILayout.BeginHorizontal();

            AssimilationObject = (GameObject)EditorGUILayout.ObjectField(AssimilationObject, typeof(GameObject));
            if (GUILayout.Button("Assimilate"))
            {
            }

            EditorGUILayout.EndHorizontal();

            Self.OptionAssimilateAlignGroups = EditorGUILayout.ToggleLeft(
                "Align Groups", Self.OptionAssimilateAlignGroups);

            #endregion

            #region Relationships

            EditorGUILayout.LabelField("Relationships", GUIHelper.StyleLabelHeaderHint);

            var bone_relationships_property = serializedObject.FindProperty("BoneRelationships");
            EditorGUILayout.PropertyField(bone_relationships_property, true);
            var group_relationships_property = serializedObject.FindProperty("GroupRelationships");
            EditorGUILayout.PropertyField(group_relationships_property, true);

            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        void OnAlignTransformsToFirstSpriteDescendant()
        {
            // TODO: is there a better undo-register function for this?
            var affected = new List<Transform>(Self.AllDescendants);
            Undo.RecordObjects(affected.ToArray(), "Group alignment");

            Self.AlignTransformsToFirstSpriteDescendant(
                Self.BonePrefix,
                Self.OptionOperateOnMatchingBones);
            EditorUtility.SetDirty(Self);
        }

        void OnCreateBones()
        {
            var bone_transforms = Self.GetBones(Self.BonePrefix, true);
            foreach (Transform bone_transform in bone_transforms)
                AddComponent<Bone>(bone_transform.gameObject);
        }

        void OnDestroyBones()
        {
            foreach (Bone bone in Self.GetComponentsInChildren<Bone>())
                RemoveComponent(bone);
        }
        
        void OnOrientParentBones()
        {
            IEnumerable<Transform> bones = Self.GetBones(Self.BonePrefix, Self.OptionOperateOnMatchingBones);
            foreach (Transform bone in bones)
            {
                Undo.RecordObject(bone, "Orient Parent Bones");
                Vector3 first_child_local_position = bone.GetChild(0).localPosition;
                Quaternion bone_rotation_difference = Quaternion.FromToRotation(Vector3.up, first_child_local_position);
                Quaternion bone_rotation_difference_inv = Quaternion.Inverse(bone_rotation_difference);
                bone.localRotation = bone_rotation_difference * bone.localRotation;
                foreach (Transform child in bone)
                {
                    Undo.RecordObject(child, "Orient Parent Bones");
                    child.localRotation = bone_rotation_difference_inv * child.localRotation;
                    child.localPosition = bone_rotation_difference_inv * child.localPosition;
                }
            }
            EditorUtility.SetDirty(Self);
        }

        void OnReachParentBones()
        {
            IEnumerable<Bone> bones = Self.GetComponentsInChildren<Bone>();
            foreach (Bone bone in bones)
            {
                Undo.RecordObject(bone, "Reach Parent Bones");
                Bone first_child_bone = TransformUtils.GetComponentInChildrenExclusively<Bone>(bone.transform);
                if (first_child_bone)
                {
                    Vector3 first_child_bone_local_position = first_child_bone.transform.localPosition;
                    bone.length = first_child_bone_local_position.magnitude;
                }
            }
            EditorUtility.SetDirty(Self);
        }

        void OnLinkBones()
        {
            Undo.RegisterCompleteObjectUndo(Self, "Link Bones");
            var link_bones = SkeletonHelper.Link(
                Self.BoneRelationships,
                SetParentCurry("Link Bones"));

            foreach (var t in link_bones) ; // Mind the gap

            Self.BoneRelationships = new SkeletonHelper.Relationship[0];
            EditorUtility.SetDirty(Self);
        }

        void OnUnlinkBones()
        {
            Undo.RegisterCompleteObjectUndo(Self, "Unlink Bones");

            IEnumerable<Transform> bones = Self.GetBones(
                Self.BonePrefix,
                Self.OptionOperateOnMatchingBones);

            var unlink_bones = SkeletonHelper.Unlink(
                Self.BoneRelationships,
                bones,
                Self.transform,
                SetParentCurry("Unlink Bones"));
            Self.BoneRelationships = new List<SkeletonHelper.Relationship>(unlink_bones).ToArray();
            EditorUtility.SetDirty(Self);
        }

        void OnLinkGroups()
        {
            Undo.RegisterCompleteObjectUndo(Self, "Link Groups");
            var link_groups = SkeletonHelper.Link(
                Self.GroupRelationships,
                SetParentCurry("Link Groups"));
            foreach (var t in link_groups) ;
            Self.GroupRelationships = new SkeletonHelper.Relationship[0];
            EditorUtility.SetDirty(Self);
        }

        void OnUnlinkGroups()
        {
            Undo.RegisterCompleteObjectUndo(Self, "Unlink Groups");

            IEnumerable<Transform> groups = Self.GetGroups(Self.BonePrefix, Self.OptionOperateOnMatchingBones);

            var unlink_groups = SkeletonHelper.Unlink(
                Self.GroupRelationships,
                groups,
                Self.transform,
                SetParentCurry("Unlink Groups"));
            Self.GroupRelationships = new List<SkeletonHelper.Relationship>(unlink_groups).ToArray();
            EditorUtility.SetDirty(Self);
        }

        void OnLockGroups(bool @lock = true)
        {
            IEnumerable<Transform> groups = Self.GetGroups(Self.BonePrefix, Self.OptionOperateOnMatchingBones);
            IEnumerable<Transform> group_descendants = TransformUtils.GetAllDescendants(groups);
            foreach (Transform group in group_descendants)
            {
                if (@lock)
                    group.gameObject.hideFlags |= HideFlags.NotEditable;
                else
                    group.gameObject.hideFlags &= ~HideFlags.NotEditable;
            }
        }

        void OnUnlockGroups()
        {
            OnLockGroups(false);
        }

        /// <summary>
        /// Creates a SetTransformParentDelegate that
        /// wraps Undo.SetTransformParent and curries
        /// the name argument for the undo operation
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static TransformUtils.SetTransformParentDelegate SetParentCurry(string name)
        {
            return new TransformUtils.SetTransformParentDelegate((t, parent, worldPositionStays) =>
            {
                // Warning: Undo.SetTransformParent has no option for worldPositionStays
                Undo.SetTransformParent(t, parent, name);
            });
        }

        private static T AddComponent<T>(GameObject game_object) where T : Component
        {
            return Undo.AddComponent<T>(game_object);
        }

        private static void RemoveComponent<T>(T component) where T : Component
        {
            Undo.DestroyObjectImmediate(component);
        }
    }
}
#endif
