using UnityEditor;
using UnityEngine;
using RoaringFangs.FSM;
using System.Collections.Generic;
using RoaringFangs.Attributes;

namespace RoaringFangs.FSM.Editor
{
    // Forget this mess
    // Come back to this later
    /*
    [CustomPropertyDrawer(typeof(FixedLengthAttribute))]
    public class FixedLengthPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect position_start = position;
            SerializedProperty start_property, end_property;
            start_property = property.Copy();
            end_property = property.GetEndProperty(true);
            Debug.Log(property.displayName);
            while (property.Next(false) && property != end_property)
            {
                if (property.name == "size")
                    continue;
                if (property.name == "data")
                    continue;
                //Debug.Log(property.name);
                EditorGUI.PropertyField(position, property);
                position.y += EditorGUI.GetPropertyHeight(property, null);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
    */
}