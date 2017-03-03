using UnityEngine;

using System;
using System.Linq;
using RoaringFangs.Adapters.FluffyUnderware.Curvy;
using RoaringFangs.Utility;
using RoaringFangs.Editor;

#if FLUFFYUNDERWARE_CURVY
#else

using RoaringFangs.Adapters.FluffyUnderware.Curvy;

#endif

using RoaringFangs.Attributes;

namespace RoaringFangs.Visuals
{
    [ExecuteInEditMode]
    public class CurvySplineMeshDeformer : MonoBehaviour, ISerializationCallbackReceiver
    {
        #region Properties

        [SerializeField, AutoProperty]
        private Renderer _Renderer;

        public Renderer Renderer
        {
            get { return _Renderer; }
            set
            {
                if (value == null)
                    _Renderer = GetComponent<Renderer>();
                else
                    _Renderer = value;
            }
        }

        [SerializeField, AutoProperty]
        private Material[] _ReferenceMaterials;

        public Material[] ReferenceMaterials
        {
            get
            {
                return _ReferenceMaterials;
            }
            set
            {
                _ReferenceMaterials = value;
                var instanced_materials = value
                    .Select(m => m != null ? new Material(m) : null)
                    .ToArray();
                Renderer.sharedMaterials = instanced_materials;
            }
        }

        public Material[] Materials
        {
            get
            {
                return Renderer.sharedMaterials;
            }
        }

        [SerializeField, AutoProperty]
        private Color _Color;

        public Color Color
        {
            get { return _Color; }
            set
            {
                _Color = value;
                foreach (var material in Materials.Where(m => m != null))
                    material.color = value;
            }
        }

        [SerializeField, AutoProperty]
        private Vector2 _StretchFactor;

        public Vector2 StretchFactor
        {
            get { return _StretchFactor; }
            set
            {
                var valid_materials = Materials.Where(m => m != null);
                foreach (var material in valid_materials)
                {
                    material.SetFloat("_StretchX", value.x);
                    material.SetFloat("_StretchY", value.y * _SegmentLength);
                }
                _StretchFactor = value;
                //StraightenEnds = _StraightenEnds;
            }
        }

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

        [SerializeField, AutoProperty("TargetPathSpline")]
        private MonoBehaviour _TargetPathSplineBehavior;

        public ICurvySpline TargetPathSpline
        {
            get { return (ICurvySpline)_TargetPathSplineBehavior; }
            set
            {
                _TargetPathSplineBehavior = (MonoBehaviour)value;
                _WeightsDirty = true;
            }
        }

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
                var valid_materials = Materials.Where(m => m != null);
                foreach (var material in valid_materials)
                {
                    material.SetFloat("_StretchY", _StretchFactor.y * value);
                }
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

        [SerializeField, MinMax(0f, 1f), AutoProperty]
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

        [SerializeField]
        private Positioning _Positioning;

        public Positioning Positioning
        {
            get { return _Positioning; }
            set
            {
                _Positioning = value;
                _WeightsDirty = true;
            }
        }

        private bool _WeightsDirty = true;
        private bool _MaterialsDirty = true;

        #endregion Properties

        private void Start()
        {
            // Necessary
            TargetPathSpline.Refresh();
            // Lazy initialization for properties that need it
            ReferenceMaterials = ReferenceMaterials; // Instantiates materials
            Color = Color; // Applies colors to material instances
            SegmentLength = SegmentLength; // Affects material instance stretch parameters
        }

        private int _SplinePointsChecksum;

        private void Update()
        {
            if (TargetPathSpline != null)
            {
                int spline_points_checksum = CurvyControlPointsHash((ICurvySpline)TargetPathSpline);
                if (TargetPathSpline.Dirty || _SplinePointsChecksum != spline_points_checksum || _WeightsDirty)
                {
                    UpdateMeshWeights(true, true, _WeightsDirty);
                    _SplinePointsChecksum = spline_points_checksum;
                }
            }
        }

        private static int CurvySplineSegmentHash(ICurvySplineSegment s)
        {
            return s.transform.localToWorldMatrix.GetHashCode();
        }

        private static int CurvyControlPointsHash(ICurvySpline spline)
        {
            if (spline.ControlPoints.Count == 0)
                return 0;
            var hash_codes = spline.ControlPoints
                .Where(p => p != null)
                .Select((Func<ICurvySplineSegment, int>)CurvySplineSegmentHash)
                .ToArray();
            int xor_hash = hash_codes.Aggregate((a, b) => a ^ b);
            return xor_hash;
        }

        public void UpdateMeshWeights()
        {
            UpdateMeshWeights(true, true, true);
        }

        public void UpdateMeshWeights(bool affect_position, bool affect_rotation, bool affect_scale)
        {
            if (TargetPathSpline != null && Weights != null)
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
                            t = TargetPathSpline.DistanceToTF(TargetPathSpline.Length * t);
                        tangent = TargetPathSpline.GetTangentFast(t);
                        point = TargetPathSpline.InterpolateFast(t);
                        if (x < x_min)
                            point += (x - x_min) * SegmentLength * TargetPathSpline.Length * tangent;
                        else
                            point += (x - x_max) * SegmentLength * TargetPathSpline.Length * tangent;
                    }
                    else
                    {
                        t = SegmentPathPosition + SegmentLength * x;
                        if (UseUniformSpacing)
                            t = TargetPathSpline.DistanceToTF(TargetPathSpline.Length * t);
                        tangent = TargetPathSpline.GetTangentFast(t);
                        point = TargetPathSpline.InterpolateFast(t);
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
                        rotation = TargetPathSpline.GetOrientationFast(t) * rotation_initial;
                    }

                    point += rotation * SegmentNormalShift;

                    Transform weight = Weights[i];

                    if (Positioning == Positioning.WorldSpace)
                    {
                        if (affect_position)
                            weight.position = point;
                        if (affect_rotation)
                            weight.rotation = rotation;
                    }
                    else if (Positioning == Positioning.LocalSpace)
                    {
                        if (affect_position)
                            weight.localPosition = point;
                        if (affect_rotation)
                            weight.localRotation = rotation;
                    }
                    if (affect_scale)
                        weight.localScale = SegmentScale;
                }
                _WeightsDirty = false;
            }
        }

        public void OnBeforeSerialize()
        {
            EditorUtilities.OnBeforeSerializeAutoProperties(this);
        }

        public void OnAfterDeserialize()
        {
        }
    }
}