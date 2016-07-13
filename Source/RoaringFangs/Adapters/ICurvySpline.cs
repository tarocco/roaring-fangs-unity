﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoaringFangs.Adapters.FluffyUnderware.Curvy
{
    public interface ICurvySpline
    {
        float GetNearestPointTF(Vector3 point);
        float DistanceToTF(float distance);
        Vector3 Interpolate(float tf);
        Vector3 GetTangent(float tf);
        float Length { get; }
        List<ICurvySplineSegment> ControlPoints { get; }
        bool Dirty { get; }
        Quaternion GetOrientationFast(float tf);
    }
}
