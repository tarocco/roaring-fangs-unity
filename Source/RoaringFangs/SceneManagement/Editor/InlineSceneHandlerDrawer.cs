using RoaringFangs.Attributes;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.SceneManagement.Editor
{
    [CustomPropertyDrawer(typeof(InlineSceneHandlerAttribute))]
    public class InlineSceneHandlerDrawer : InlineDrawer
    {
        protected override IEnumerable<SerializedProperty> GetInlineSerializedProperties(SerializedProperty property)
        {
            var property_scene_name = property.FindPropertyRelative("_SceneName");
            yield return property_scene_name;
            var property_load_mode = property.FindPropertyRelative("_LoadMode");
            yield return property_load_mode;
        }
    }
}