using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace RoaringFangs.SceneManagement.Editor
{
    //[CustomPropertyDrawer(typeof(ScriptableSceneHandler))]
    /*
    public class ScriptableSceneHandlerDrawer : PropertyDrawer
    {
        private SerializedProperty GetSelfSerializedProperty(UnityObject target)
        {
            var serialized_scriptable = new SerializedObject(target);
            var self_property = serialized_scriptable.FindProperty("_Self");
            return self_property;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var target = property.objectReferenceValue;
            if (target != null)
            {
                var self_property = GetSelfSerializedProperty(target);
                Rect position_reference, position_self;

                position_reference = position;
                position_reference.height = EditorGUI.GetPropertyHeight(property, label);

                position_self = position;
                position_self.yMin = position_reference.yMax;
                position_self.height = EditorGUI.GetPropertyHeight(property, label);

                EditorGUI.PropertyField(position_reference, property, label);
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position_self, self_property, true);
                if (EditorGUI.EndChangeCheck())
                    self_property.serializedObject.ApplyModifiedProperties();
            }
            else
                EditorGUI.PropertyField(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var target = property.objectReferenceValue;
            if (target != null)
            {
                var self_property = GetSelfSerializedProperty(target);

                float base_height = EditorGUI.GetPropertyHeight(property, label);
                float extended_height = EditorGUI.GetPropertyHeight(self_property, label, true);
                float total_height = base_height + extended_height;

                return total_height;
            }
            else
                return EditorGUI.GetPropertyHeight(property, label);
        }
    }
    */
}