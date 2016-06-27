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

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RoaringFangs.Utility;

namespace RoaringFangs.Animation.Editor
{
    [CustomEditor(typeof(MutexHelper))]
    public class MutexControlEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MutexHelper self = (MutexHelper)target;
            TargetGroupBehavior[] groups = self.Controls.ToArray();
            string[] group_names = groups.Select(g => g.name).ToArray();
            int index_selected = Math.Max(0, Array.IndexOf(groups, self.Selected));
            index_selected = EditorGUILayout.Popup(index_selected, group_names);
            TargetGroupBehavior selected;
            if (index_selected < groups.Length)
                selected = groups[index_selected];
            else
                selected = null;
            if (self.Selected != selected)
            {
                Undo.RecordObjects(self.Controls.Select(c=>c.gameObject).ToArray(), "Select Target Group");
                self.Selected = selected;
            }

            #region I give up
            /*
            if (selected != null && self.Selected != selected)
            {
                var prop_selected = serializedObject.FindProperty("_Selected__encoded");
                prop_selected.floatValue = new EncodedObjectReference<TargetGroupBehavior>(selected).GetEncodedID();
                //Debug.Log("FOO: " + selected.GetInstanceID());
                //Debug.Log("BAR: " + new EncodedObjectReference<TargetGroupBehavior>(self.transform, prop_selected.floatValue).GetInstanceID());
                
                var anim_clip = AnimationWindowUtils.GetCurrentActiveAnimationClip();
                var bindings = AnimationUtility.GetCurveBindings(anim_clip);
                var selected_encoded_bindings = bindings.Where(b => b.propertyName == "_Selected__encoded");
                foreach (var binding in selected_encoded_bindings)
                {
                    var reference_curve = AnimationUtility.GetEditorCurve(anim_clip, binding);
                    for (int i = 0; i < reference_curve.length; i++)
                    {
                        Keyframe k = reference_curve.keys[i];
                        k.inTangent = float.PositiveInfinity;
                        k.outTangent = float.PositiveInfinity;
                        k.tangentMode = 31;
                        reference_curve.keys[i] = k;
                        Debug.Log(k);
                    }
                    AnimationUtility.SetEditorCurve(anim_clip, binding, reference_curve);
                }
            }
            */
            #endregion

            DrawPropertiesExcluding(serializedObject);
            serializedObject.ApplyModifiedProperties();
        }
    }
}