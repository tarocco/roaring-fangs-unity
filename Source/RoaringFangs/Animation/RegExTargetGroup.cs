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

#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;

using RoaringFangs.Utility;

using RoaringFangs.Attributes;

namespace RoaringFangs.Animation
{
	public class RegExTargetGroup : TargetGroupBehavior
	{
		#region Properties

		[SerializeField, AutoProperty (Delayed = true)]
		private string _Pattern;

		public string Pattern {
			get { return _Pattern; }
			set {
				_Pattern = value;
				SetTargets (CollectTargets ());
			}
		}

		#endregion

		#region Subject Descendants and Paths

		private IEnumerable<TransformUtils.ITransformDP> _SubjectDescendantsAndPaths;

		private IEnumerable<TransformUtils.ITransformDP> SubjectDescendantsAndPaths {
			get {
				if (_SubjectDescendantsAndPaths == null) {
					// Lazily tell the control manager to invalidate the subject descendants
					var manager = TransformUtils.GetComponentInParent<ControlManager> (transform, true);
					if (manager)
						manager.NotifyControlGroupsOfSubjectDescendants ();
				}
				return _SubjectDescendantsAndPaths;
			}
			set {
				_SubjectDescendantsAndPaths = value;
			}
		}

		#endregion

		#region Targets

		#region Cached Target Transforms

		[SerializeField, HideInInspector]
		private TransformUtils.TransformD[] _Targets;

		public override IEnumerable<TransformUtils.ITransformD> Targets {
			get {
				if (_Targets != null) {
					foreach (var target in _Targets)
						yield return target;
				}
			}
		}

		#endregion

		private static IEnumerable<TransformUtils.ITransformD> FindMatchingTransformsD (IEnumerable<TransformUtils.ITransformDP> descendants, string regex_pattern)
		{
			Regex regex = new Regex (regex_pattern);
			var descendants_array = descendants.ToArray ();
			foreach (var tp in descendants_array) {
				Match match = regex.Match (tp.Path);
				if (match.Success) {
					var groups = match.Groups;
					if (groups.Count == 1) {
						yield return new TransformUtils.TransformD (tp.Transform, tp.Depth);
					} else if (groups.Count > 1) {
						var group1 = groups [1];
						var grouped_parent_path = tp.Path.Substring (0, group1.Index + group1.Length);
						//Debug.Log("Grouped: " + grouped_parent_path);
						int parent_depth = grouped_parent_path.Count (c => c == '/');
						Transform parent = tp.Transform;
						for (int d = tp.Depth; d > parent_depth; d--)
							parent = parent.parent; // lol
						yield return new TransformUtils.TransformD (parent, parent_depth);
					}
				}
				//Debug.Log(tp.Path);
			}
		}

		private IEnumerable<TransformUtils.ITransformD> CollectTargets ()
		{
			return FindMatchingTransformsD (SubjectDescendantsAndPaths, Pattern);
		}
		// Can't use the Targets set accessor because superclass lacks it
		private void SetTargets (IEnumerable<TransformUtils.ITransformD> value)
		{
			if (value != null)
				_Targets = value.Select (t => new TransformUtils.TransformD (t.Transform, t.Depth)).ToArray ();
			else
				_Targets = null;
		}

		#endregion

		#region Other Methods

		public override void OnSubjectChanged (IEnumerable<TransformUtils.ITransformDP> subject_descendants_and_paths)
		{
			if (subject_descendants_and_paths != null) {
				SubjectDescendantsAndPaths = subject_descendants_and_paths.ToArray ();
				SetTargets (CollectTargets ());
			} else {
				SubjectDescendantsAndPaths = null;
				SetTargets (null);
			}
		}

        
		#endregion

		#region Editor Menus

		#if UNITY_EDITOR
		private static Type HierarchyWindow = typeof(EditorWindow).Assembly.GetType ("UnityEditor.SceneHierarchyWindow");

		[MenuItem ("Sprites And Bones/Animation/RegEx Target Group", false, 0)]
		[MenuItem ("GameObject/Sprites And Bones/RegEx Target Group", false, 0)]
		[MenuItem ("CONTEXT/ControlManager/RegEx Target Group", false, 25)]
		public static RegExTargetGroup Create ()
		{
			GameObject selected = Selection.activeGameObject;
			ControlManager manager;
			if (selected != null)
				manager = selected.GetComponent<ControlManager> ();
			else
				manager = null;
			if (manager == null)
				throw new Exception ("You must select a control for this target group.");

			GameObject regex_target_group_object = new GameObject ("New RegEx Target Group");
			Undo.RegisterCreatedObjectUndo (regex_target_group_object, "Add Regex Target Group");
			RegExTargetGroup regex_target_group = regex_target_group_object.AddComponent<RegExTargetGroup> ();

			Undo.SetTransformParent (regex_target_group_object.transform, selected.transform, "Add RegEx Target Group");
			Selection.activeGameObject = regex_target_group_object;
			var hierarchy_window = EditorWindow.GetWindow (HierarchyWindow);
			if (hierarchy_window != null) {
				hierarchy_window.Focus ();
				// TODO: figure out why this isn't working!
				var rename_go = HierarchyWindow.GetMethod ("RenameGO", BindingFlags.NonPublic | BindingFlags.Instance);
				rename_go.Invoke (hierarchy_window, null);
			}
			return regex_target_group;
		}
		#endif
		#endregion
	}
}