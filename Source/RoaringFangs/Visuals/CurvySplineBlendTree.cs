using RoaringFangs.Adapters.FluffyUnderware.Curvy;
using RoaringFangs.Attributes;
using RoaringFangs.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoaringFangs.Visuals
{
    [ExecuteInEditMode]
    public class CurvySplineBlendTree : MonoBehaviour, ICurvySpline
    {
        [SerializeField, AutoProperty("Splines")]
        private List<MonoBehaviour> _SplineBehaviors;

        public List<ICurvySpline> Splines
        {
            get { return _SplineBehaviors.Cast<ICurvySpline>().ToList(); }
            set
            {
                _SplineBehaviors = value.Cast<MonoBehaviour>().ToList();
                _Dirty = true;
            }
        }

        private int InterpolantMax
        {
            get
            {
                if(_SplineBehaviors != null)
                    return (int)_SplineBehaviors.LongCount() - 1;
                return 0;
            }
        }

        [SerializeField, AutoRange(0f, "InterpolantMax")]
        private float _Interpolant;

        public float Interpolant
        {
            get { return _Interpolant; }
            set
            {
                _Interpolant = value;
                _Dirty = true;
            }
        }

        private static void GetIndicesAndSegmentInterpolant(
            int count,
            float t,
            out int index_a,
            out int index_b,
            out float t_segment)
        {
            float t_clamped = Mathf.Clamp(t, 0f, count - 1);
            int idx = Mathf.FloorToInt(t_clamped);
            index_a = Mathf.Clamp(idx, 0, count - 2);
            index_b = index_a + 1;
            t_segment = Mathf.Clamp01(t - (float)index_a);
        }

        public static float LerpAcross(IEnumerable<float> values, int count, float t)
        {
            int index_a, index_b;
            float t_segment;
            GetIndicesAndSegmentInterpolant(
                count,
                t,
                out index_a,
                out index_b,
                out t_segment);
            float a = values.Skip(index_a).First();
            float b = values.Skip(index_b).First();
            return Mathf.Lerp(a, b, t_segment);
        }

        public static Vector3 LerpAcross(IEnumerable<Vector3> values, int count, float t)
        {
            int index_a, index_b;
            float t_segment;
            GetIndicesAndSegmentInterpolant(
                count,
                t,
                out index_a,
                out index_b,
                out t_segment);
            Vector3 a = values.Skip(index_a).First();
            Vector3 b = values.Skip(index_b).First();
            return Vector3.Lerp(a, b, t_segment);
        }

        public static Quaternion SlerpAcross(IEnumerable<Quaternion> values, int count, float t)
        {
            int index_a, index_b;
            float t_segment;
            GetIndicesAndSegmentInterpolant(
                count,
                t,
                out index_a,
                out index_b,
                out t_segment);
            Quaternion a = values.Skip(index_a).First();
            Quaternion b = values.Skip(index_b).First();
            return Quaternion.Slerp(a, b, t_segment);
        }

        public List<ICurvySplineSegment> ControlPoints
        {
            get
            {
                return Splines.SelectMany(s => s.ControlPoints).ToList();
            }
        }

        private bool _Dirty;

        public bool Dirty
        {
            get
            {
                return _Dirty || Splines.Any(s => s.Dirty);
            }
        }

        public float Length
        {
            get
            {
                if (Splines.Count == 0)
                    return 0f;
                if (Splines.Count == 1)
                    return Splines[0].Length;
                return LerpAcross(
                    Splines.Select(s => s.Length),
                    Splines.Count,
                    Interpolant);
            }
        }

        public float DistanceToTF(float distance)
        {
            if (Splines.Count == 0)
                return 0f;
            if (Splines.Count == 1)
                return Splines[0].DistanceToTF(distance);
            return LerpAcross(
                Splines.Select(s => s.DistanceToTF(distance)),
                Splines.Count,
                Interpolant);
        }

        public float GetNearestPointTF(Vector3 point)
        {
            if (Splines.Count == 0)
                return 0f;
            if (Splines.Count == 1)
                return Splines[0].GetNearestPointTF(point);
            return LerpAcross(
                Splines.Select(s => s.GetNearestPointTF(point)),
                Splines.Count,
                Interpolant);
        }

        public Quaternion GetOrientationFast(float tf)
        {
            if (Splines.Count == 0)
                return Quaternion.identity;
            if (Splines.Count == 1)
                return Splines[0].GetOrientationFast(tf);
            return SlerpAcross(
                Splines.Select(s => s.GetOrientationFast(tf)),
                Splines.Count,
                Interpolant);
        }

        public Vector3 GetTangent(float tf)
        {
            if (Splines.Count == 0)
                return Vector3.zero;
            if (Splines.Count == 1)
                return Splines[0].GetTangent(tf);
            return LerpAcross(
                Splines.Select(s => s.GetTangent(tf)),
                Splines.Count,
                Interpolant);
        }

        public Vector3 GetTangentFast(float tf)
        {
            if (Splines.Count == 0)
                return Vector3.zero;
            if (Splines.Count == 1)
                return Splines[0].GetTangentFast(tf);
            return LerpAcross(
                Splines.Select(s => s.GetTangentFast(tf)),
                Splines.Count,
                Interpolant);
        }

        public Vector3 Interpolate(float tf)
        {
            if (Splines.Count == 0)
                return Vector3.zero;
            if (Splines.Count == 1)
                return Splines[0].Interpolate(tf);
            return LerpAcross(
                Splines.Select(s => s.Interpolate(tf)),
                Splines.Count,
                Interpolant);
        }

        public Vector3 InterpolateFast(float tf)
        {
            if (Splines.Count == 0)
                return Vector3.zero;
            if (Splines.Count == 1)
                return Splines[0].InterpolateFast(tf);
            return LerpAcross(
                Splines.Select(s => s.InterpolateFast(tf)),
                Splines.Count,
                Interpolant);
        }

        public void Refresh()
        {
            foreach (var spline in Splines)
                spline.Refresh();
        }

        void LateUpdate()
        {
            if (Dirty)
            {
                Refresh();
                _Dirty = false;
            }
        }

#if UNITY_EDITOR
        [SerializeField, AutoRange(1f, 64f)]
        private int _PreviewSegments = 24;

        public int PreviewSegments
        {
            get { return _PreviewSegments; }
            set { _PreviewSegments = value; }
        }

        [SerializeField]
        private Color _PreviewColor = Color.white;

        public Color PreviewColor
        {
            get { return _PreviewColor; }
            set { _PreviewColor = value; }
        }

        void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            float t_previous = 0f;
            for (int i = 1; i <= PreviewSegments; i++)
            {
                Gizmos.color = PreviewColor;
                float t = (float)i / (float)(PreviewSegments);
                Gizmos.DrawLine(
                        InterpolateFast(t_previous),
                        InterpolateFast(t)
                    );
                t_previous = t;
            }
        }
#endif
    }
}