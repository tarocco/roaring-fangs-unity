using RoaringFangs.Attributes;
using System;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.Animation.Editor
{
    [CustomEditor(typeof(RefBoolBehaviorBase), true)]
    [CanEditMultipleObjects]
    public class RefBoolControllerEditor : UnityEditor.Editor
    {
        private static readonly string ValueObjectName = "_ValueObject";
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObjects(serializedObject.targetObjects, "Modify RefBool Value");
            var script_property = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(script_property);
            var value_object_property = serializedObject.FindProperty(ValueObjectName);
            var value_object = value_object_property.objectReferenceValue;
            var value = value_object == RefBool.True;
            var label = new GUIContent("Value");
            var value_next = EditorGUILayout.Toggle(label, value);
            var value_object_next = value_next ? RefBool.True : RefBool.False;
            value_object_property.objectReferenceValue = value_object_next;
            DrawPropertiesExcluding(serializedObject, "m_Script", ValueObjectName);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}