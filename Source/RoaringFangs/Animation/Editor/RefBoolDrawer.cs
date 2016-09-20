using RoaringFangs.Attributes;
using System;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.Animation.Editor
{
    [CustomPropertyDrawer(typeof(RefBool))]
    public class RefBoolDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var serialied_value_object = property.FindPropertyRelative("_ValueObject");
            var value_object = serialied_value_object.objectReferenceValue;
            bool value = value_object == RefBool.True;
            bool next_value = EditorGUI.Toggle(position, "Value", value);
            if (value != next_value)
            {
                var next_value_object = next_value ? RefBool.True : RefBool.False;
                serialied_value_object.objectReferenceValue = next_value_object;
                var serialized_object = property.serializedObject;
                serialized_object.ApplyModifiedProperties();
            }
        }
    }
}