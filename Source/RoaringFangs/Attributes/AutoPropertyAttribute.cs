using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace RoaringFangs.Attributes
{
    public class AutoPropertyAttribute : PropertyAttribute
    {
        private const BindingFlags DefaultFlags =
            BindingFlags.GetProperty |
            BindingFlags.SetProperty |
            BindingFlags.Public |
            BindingFlags.NonPublic;
        public PropertyInfo PropertyInfo;
        public AutoPropertyAttribute(Type type, string property_name) :
            base()
        {
            if (type != null)
                PropertyInfo = type.GetProperty(property_name, DefaultFlags);
        }
        public AutoPropertyAttribute() :
            this(null, null)
        {
        }
    }
}