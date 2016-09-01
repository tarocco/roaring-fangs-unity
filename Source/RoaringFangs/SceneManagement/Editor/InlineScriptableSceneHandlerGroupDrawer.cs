using RoaringFangs.Attributes;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace RoaringFangs.SceneManagement.Editor
{
    [CustomPropertyDrawer(typeof(InlineScriptableSceneHandlerGroupAttribute))]
    public class InlineScriptableSceneHandlerGroupDrawer : InlineObjectReferencePropertiesDrawer
    {
        protected override IEnumerable<SerializedProperty> GetInlineSerializedProperties(SerializedObject serialized_object)
        {
            var property_self = serialized_object.FindProperty("_Self");
            var property_configuration_object = property_self.FindPropertyRelative("_ConfigurationObject");
            yield return property_configuration_object;
            var property_scene_handlers = property_self.FindPropertyRelative("_SceneHandlers");
            foreach (var @object in property_scene_handlers)
            {
                var property_scene_handler = (SerializedProperty)@object;
                yield return property_scene_handler;
            }
            //yield return property_scene_handlers;
        }
    }
}