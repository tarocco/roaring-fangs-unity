using RoaringFangs.Attributes;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace RoaringFangs.Utility
{
    [CreateAssetMenu(
        fileName = "New ScriptableMarker",
        menuName = "Roaring Fangs/Create ScriptableMarker")]
    public class ScriptableMarker: ScriptableObject
    {
    }
}