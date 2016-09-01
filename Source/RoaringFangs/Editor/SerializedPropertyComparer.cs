using System.Collections.Generic;
using UnityEditor;

namespace RoaringFangs.Editor
{
    /// <summary>
    /// This is why we can't have nice things
    /// </summary>
    public class SerializedPropertyComparer : Comparer<SerializedProperty>
    {
        public override int Compare(SerializedProperty x, SerializedProperty y)
        {
            return string.Compare(x.propertyPath, y.propertyPath);
        }
    }
}