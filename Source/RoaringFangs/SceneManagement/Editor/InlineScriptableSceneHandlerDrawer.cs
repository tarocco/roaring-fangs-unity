using RoaringFangs.Attributes;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace RoaringFangs.SceneManagement.Editor
{
    [CustomPropertyDrawer(typeof(InlineScriptableSceneHandlerAttribute))]
    public class InlineScriptableSceneHandlerDrawer : InlineObjectReferencePropertiesDrawer
    {
        protected override IEnumerable<SerializedProperty> GetInlineSerializedProperties(SerializedObject serialized_object)
        {
            var property_self = serialized_object.FindProperty("_Self");
            var property_scene_name = property_self.FindPropertyRelative("_SceneName");
            yield return property_scene_name;
            var property_load_mode = property_self.FindPropertyRelative("_LoadMode");
            yield return property_load_mode;
        }
    }
}