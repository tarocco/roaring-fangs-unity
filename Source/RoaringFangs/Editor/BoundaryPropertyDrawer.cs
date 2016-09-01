using RoaringFangs.Attributes;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.FSM.Editor
{
    [CustomPropertyDrawer(typeof(BoundaryDrawerAttribute))]
    public class EnumSetPropertyDrawer : PropertyDrawer
    {
        private struct StateMenuCallbackArgs
        {
            public System.Type EnumType;
            public object StateObject;
            public SerializedProperty StateNamesProperty;

            public StateMenuCallbackArgs(
                System.Type enum_type,
                object state_object,
                SerializedProperty state_names_property)
            {
                EnumType = enum_type;
                StateObject = state_object;
                StateNamesProperty = state_names_property;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (UnityEngine.GUI.Button(position, label, EditorStyles.layerMaskField))
            {
                var menu = new GenericMenu();
                var attr = (BoundaryDrawerAttribute)attribute;
                var enum_type = attr.EnumType;
                var enum_names_array = System.Enum.GetNames(attr.EnumType);
                var state_names_property = property.FindPropertyRelative("_StateNames");

                var states_selected = new HashSet<object>();
                var names_selected = new HashSet<string>();

                for (int i = 0; i < state_names_property.arraySize; i++)
                {
                    var state_name_property = state_names_property.GetArrayElementAtIndex(i);
                    // enumValueIndex isn't very helpful...
                    //var enum_object = System.Enum.ToObject(enum_type, state_property.intValue);
                    //var enum_name = System.Enum.GetName(enum_type, enum_object);
                    var enum_name = state_name_property.stringValue;
                    var enum_object = System.Enum.Parse(enum_type, enum_name);

                    states_selected.Add(enum_object);
                    names_selected.Add(enum_name);
                }
                
                foreach (var enum_name in enum_names_array)
                {
                    GUIContent content = new GUIContent(enum_name);
                    var state_object = System.Enum.Parse(enum_type, enum_name);
                    var args = new StateMenuCallbackArgs(enum_type, state_object, state_names_property);
                    if (names_selected.Contains(enum_name))
                        menu.AddItem(content, true, OnRemoveState, args);
                    else
                        menu.AddItem(content, false, OnAddState, args);
                }

                menu.ShowAsContext();
            }
        }

        private void OnAddState(object o)
        {
            var args = (StateMenuCallbackArgs)o;
            var enum_type = args.EnumType;
            var selected_state = args.StateObject;
            var state_names_property = args.StateNamesProperty;
            int index = state_names_property.arraySize;
            state_names_property.InsertArrayElementAtIndex(index);
            var element = state_names_property.GetArrayElementAtIndex(index);
            element.stringValue = System.Enum.GetName(enum_type, selected_state);
            element.serializedObject.ApplyModifiedProperties();
        }

        private void OnRemoveState(object o)
        {
            var args = (StateMenuCallbackArgs)o;
            var enum_type = args.EnumType;
            var selected_state = args.StateObject;
            var selected_state_name = System.Enum.GetName(enum_type, selected_state);
            var states_property = args.StateNamesProperty;
            int size = states_property.arraySize;
            for (int i = 0; i < size; i++)
            {
                var element = states_property.GetArrayElementAtIndex(i);
                if(element.stringValue == selected_state_name)
                {
                    states_property.DeleteArrayElementAtIndex(i);
                    element.serializedObject.ApplyModifiedProperties();
                    break;
                }
            }
            
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}