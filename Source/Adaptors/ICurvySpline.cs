using UnityEngine;
using System.Collections;

namespace RoaringFangs.Adaptors.FluffyUnderware.Curvy
{
    public interface ICurvySpline
    {
        float GetNearestPointTF(Vector3 point);
        Vector3 GetTangent(float tf);
    }
}
