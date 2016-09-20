using RoaringFangs.Attributes;
using RoaringFangs.Utility;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.Animation
{
    public interface IRefBool
    {
        bool? Value { get; }
    }
}