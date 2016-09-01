using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.FSM.Editor
{
    public abstract class StateMachineEditor<TStateEnum> : UnityEditor.Editor
        where TStateEnum : struct, IConvertible
    {
        public override void OnInspectorGUI()
        {
            var state_machine = target as StateMachine<TStateEnum>;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinHeight(96f)))
            {
                var style = new GUIStyle(UnityEngine.GUI.skin.label);
                style.fontSize = 16;
                GUILayout.Label(state_machine.CurrentState.ToString(), style);

                var enum_next_values = state_machine.GetNextStates.ToArray();
                var enum_next_strings = enum_next_values.Select(e => e.ToString()).ToArray();

                int index = Array.IndexOf(enum_next_values, state_machine.CurrentState);
                int x_count = Mathf.FloorToInt(Screen.width / 128f);
                var button_style = UnityEngine.GUI.skin.button;
                int next_index = GUILayout.SelectionGrid(index, enum_next_strings, x_count);
                if (next_index != index)
                {
                    var next_state = enum_next_values[next_index];
                    state_machine.CurrentState = next_state;
                }
            }
            DrawPropertiesExcluding(serializedObject, "_CurrentState");
            EditorUtility.SetDirty(serializedObject.targetObject);
            serializedObject.ApplyModifiedProperties();
            
        }
    }
}