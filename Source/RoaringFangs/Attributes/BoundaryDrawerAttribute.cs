using System;
using UnityEngine;

namespace RoaringFangs.Attributes
{
    public class BoundaryDrawerAttribute : PropertyAttribute
    {
        public readonly Type EnumType;
        public BoundaryDrawerAttribute(Type enum_type)
        {
            EnumType = enum_type;
        }
    }
}