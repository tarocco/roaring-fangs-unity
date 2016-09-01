using UnityEditor;
using UnityEngine;
using RoaringFangs.FSM;

namespace RoaringFangs.FSM.Editor
{
    [CustomPropertyDrawer(typeof(StateInfoEntry))]
    public class StateInfoEntryPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var state_property = property.FindPropertyRelative("State");
            var state_name = state_property.stringValue;
            var state_label = new GUIContent(state_name);
            var info_property = property.FindPropertyRelative("Info");
            EditorGUI.PropertyField(position, info_property, state_label, true);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var info_property = property.FindPropertyRelative("Info");
            return EditorGUI.GetPropertyHeight(info_property, label);
        }
    }
}