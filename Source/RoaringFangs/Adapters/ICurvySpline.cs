using RoaringFangs.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace RoaringFangs.Adapters.FluffyUnderware.Curvy
{
    public interface ICurvySpline : IHasTransform
    {
        float GetNearestPointTF(Vector3 point);

        float DistanceToTF(float distance);

        Vector3 Interpolate(float tf);

        Vector3 InterpolateFast(float tf);

        Vector3 GetTangent(float tf);

        Vector3 GetTangentFast(float tf);

        float Length { get; }
        List<ICurvySplineSegment> ControlPoints { get; }
        bool Dirty { get; }

        Quaternion GetOrientationFast(float tf);

        void Refresh();
    }
}