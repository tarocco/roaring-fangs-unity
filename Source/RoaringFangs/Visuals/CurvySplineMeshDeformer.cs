using UnityEngine;

using System;
using System.Linq;

#if FLUFFYUNDERWARE_CURVY
using FluffyUnderware.Curvy;
#else

using RoaringFangs.Adapters.FluffyUnderware.Curvy;

#endif

using RoaringFangs.Attributes;

namespace RoaringFangs.Visuals
{
    [ExecuteInEditMode]
    public class CurvySplineMeshDeformer : MonoBehaviour
    {
        #region Properties

        [SerializeField, AutoProperty]
        private Transform[] _Weights;

        public Transform[] Weights
        {
            get { return _Weights; }
            set
            {
                _Weights = value;
                _WeightsDirty = true;
            }
        }

        [SerializeField, AutoProperty]
#if FLUFFYUNDERWARE_CURVY
        private CurvySpline _PathSpline;
        public CurvySpline PathSpline
        {
            get { return _PathSpline; }
            set
            {
                _PathSpline = value;
                _WeightsDirty = true;
            }
        }
#else
        private ICurvySpline _PathSpline;

        public ICurvySpline PathSpline
        {
            get { return _PathSpline; }
            set
            {
                _PathSpline = value;
                _WeightsDirty = true;
            }
        }

#endif

        [SerializeField, AutoProperty]
        private float _SegmentPathPosition = 0f;

        public float SegmentPathPosition
        {
            get { return _SegmentPathPosition; }
            set
            {
                _SegmentPathPosition = value;
                _WeightsDirty = true;
            }
        }

        [SerializeField, AutoProperty]
        private float _SegmentLength = 1f;

        public float SegmentLength
        {
            get { return _SegmentLength; }
            set
            {
                _SegmentLength = value;
                _WeightsDirty = true;
            }
        }

        [SerializeField, AutoProperty]
        private float _SegmentAngle = 0f;

        public float SegmentAngle
        {
            get { return _SegmentAngle; }
            set
            {
                _SegmentAngle = value;
                _WeightsDirty = true;
            }
        }

        [SerializeField, AutoProperty]
        private Vector3 _SegmentForwardVector = Vector3.forward;

        public Vector3 SegmentForwardVector
        {
            get { return _SegmentForwardVector; }
            set
            {
                _SegmentForwardVector = value;
                _WeightsDirty = true;
            }
        }

        [SerializeField, AutoProperty]
        private Vector3 _SegmentScale = Vector3.one;

        public Vector3 SegmentScale
        {
            get { return _SegmentScale; }
            set
            {
                _SegmentScale = value;
                _WeightsDirty = true;
            }
        }

        [SerializeField, AutoProperty]
        private Vector3 _SegmentNormalShift = Vector3.zero;

        public Vector3 SegmentNormalShift
        {
            get { return _SegmentNormalShift; }
            set
            {
                _SegmentNormalShift = value;
                _WeightsDirty = true;
            }
        }

        [SerializeField, AutoProperty]
        private bool _SegmentUseFixedAngle = false;

        public bool SegmentUseFixedAngle
        {
            get { return _SegmentUseFixedAngle; }
            set
            {
                _SegmentUseFixedAngle = value;
                _WeightsDirty = true;
            }
        }

        [SerializeField, AutoProperty]
        private bool _SegmentFlipWeightDirection = false;

        public bool SegmentFlipWeightDirection
        {
            get { return _SegmentFlipWeightDirection; }
            set
            {
                _SegmentFlipWeightDirection = value;
                _WeightsDirty = true;
            }
        }

        [Tooltip("Experimental feature to evenly space weights along the length of the spline.")]
        [SerializeField, AutoProperty]
        private bool _UseUniformSpacing = false;

        public bool UseUniformSpacing
        {
            get { return _UseUniformSpacing; }
            set
            {
                _UseUniformSpacing = value;
                _WeightsDirty = true;
            }
        }

        [SerializeField, AutoMinMax(0f, 1f)]
        private Vector2 _StraightenEnds = new Vector2(0f, 1f);

        public Vector2 StraightenEnds
        {
            get { return _StraightenEnds; }
            set
            {
                _StraightenEnds = value;
                _StraightenEnds.x = Mathf.Min(value.x, value.y);
                _WeightsDirty = true;
            }
        }

        private bool _WeightsDirty;

        #endregion Properties

        private void Start()
        {
            _WeightsDirty = true;
        }

        private int _SplinePointsChecksum;

        private void Update()
        {
            if (PathSpline != null)
            {
                if (PathSpline.Dirty)
                    PathSpline.Refresh();
                int spline_points_checksum = CurvyControlPointsHash(PathSpline);
                if (_SplinePointsChecksum != spline_points_checksum || _WeightsDirty)
                {
                    UpdateMeshWeights(true, true, _WeightsDirty);
                    _SplinePointsChecksum = spline_points_checksum;
                }
            }
        }

#if FLUFFYUNDERWARE_CURVY
        private static int CurvySplineSegmentHash(CurvySplineSegment s)
        {
            return s.transform.localToWorldMatrix.GetHashCode();
        }
        private static int CurvyControlPointsHash(CurvySpline spline)
        {
            return spline.ControlPoints
                .Select((Func<CurvySplineSegment, int>)CurvySplineSegmentHash)
                .Aggregate((a, b) => a ^ b);
        }
#else

        private static int CurvySplineSegmentHash(ICurvySplineSegment s)
        {
            return s.transform.localToWorldMatrix.GetHashCode();
        }

        private static int CurvyControlPointsHash(ICurvySpline spline)
        {
            return spline.ControlPoints
                .Select((Func<ICurvySplineSegment, int>)CurvySplineSegmentHash)
                .Aggregate((a, b) => a ^ b);
        }

#endif

        public void UpdateMeshWeights()
        {
            UpdateMeshWeights(true, true, true);
        }

        public void UpdateMeshWeights(bool affect_position, bool affect_rotation, bool affect_scale)
        {
            if (PathSpline != null && Weights != null)
            {
                float weights_length_m1 = (float)Weights.Length - 1f;
                Vector3 point, tangent;
                Quaternion rotation, rotation_initial;

                for (int i = 0; i < Weights.Length; i++)
                {
                    float t;
                    float x = (float)i / weights_length_m1;
                    float x_min = StraightenEnds.x;
                    float x_max = StraightenEnds.y;
                    if (x < x_min || x > x_max)
                    {
                        if (x < x_min)
                            t = SegmentPathPosition + SegmentLength * x_min;
                        else
                            t = SegmentPathPosition + SegmentLength * x_max;
                        if (UseUniformSpacing)
                            t = PathSpline.DistanceToTF(PathSpline.Length * t);
                        tangent = PathSpline.GetTangent(t);
                        point = PathSpline.Interpolate(t);
                        if (x < x_min)
                            point += (x - x_min) * SegmentLength * PathSpline.Length * tangent;
                        else
                            point += (x - x_max) * SegmentLength * PathSpline.Length * tangent;
                    }
                    else
                    {
                        t = SegmentPathPosition + SegmentLength * x;
                        if (UseUniformSpacing)
                            t = PathSpline.DistanceToTF(PathSpline.Length * t);
                        tangent = PathSpline.GetTangent(t);
                        point = PathSpline.Interpolate(t);
                    }
                    rotation_initial = Quaternion.AngleAxis(SegmentAngle, SegmentForwardVector);
                    if (SegmentFlipWeightDirection)
                    {
                        Quaternion flip = Quaternion.FromToRotation(SegmentForwardVector, -SegmentForwardVector);
                        rotation_initial = rotation_initial * flip;
                    }
                    if (SegmentUseFixedAngle)
                    {
                        rotation = rotation_initial;
                    }
                    else
                    {
                        rotation = rotation_initial * PathSpline.GetOrientationFast(t);
                    }

                    point += rotation * SegmentNormalShift;

                    Transform weight = Weights[i];
                    if (affect_position)
                        weight.localPosition = point;
                    if (affect_rotation)
                        weight.localRotation = rotation;
                    if (affect_scale)
                        weight.localScale = SegmentScale;
                }
                _WeightsDirty = false;
            }
        }
    }
}