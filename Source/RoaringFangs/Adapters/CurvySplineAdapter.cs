#if FLUFFYUNDERWARE_CURVY

using System;
using System.Collections.Generic;
using RoaringFangs.Adapters.FluffyUnderware.Curvy;
using RoaringFangs.Attributes;
using RoaringFangs.Utility;
using UnityEngine;
using FluffyUnderware.Curvy;
using System.Linq;
using RoaringFangs.Editor;

namespace RoaringFangs.Adapters
{
    [RequireComponent(typeof(CurvySpline))]
    public class CurvySplineAdapter : MonoBehaviour, ICurvySpline, ISerializationCallbackReceiver
    {
        [SerializeField]
        private CurvySpline _Self;

        public CurvySpline Self
        {
            get { return _Self; }
            set { _Self = value; }
        }

        [SerializeField]
        private bool _UseWorldCoordinates;

        public bool UseWorldCoordinates
        {
            get { return _UseWorldCoordinates; }
            protected set { _UseWorldCoordinates = value; }
        }

        public List<ICurvySplineSegment> ControlPoints
        {
            get
            {
                return Self.ControlPoints
                    .Select(p => p.gameObject.GetComponent<ICurvySplineSegment>())
                    .ToList();
            }
        }

        public bool Dirty
        {
            get
            {
                return Self.Dirty;
            }
        }

        public float Length
        {
            get
            {
                return Self.Length;
            }
        }

        public float DistanceToTF(float distance)
        {
            return Self.DistanceToTF(distance);
        }

        public float GetNearestPointTF(Vector3 point)
        {
            point = Self.transform.worldToLocalMatrix.MultiplyPoint(point);
            return Self.GetNearestPointTF(point);
        }

        public Quaternion GetOrientationFast(float tf)
        {
            if (UseWorldCoordinates)
            {
                return
                    TRS.GetRotation(Self.transform.localToWorldMatrix) *
                    Self.GetOrientationFast(tf);
            }
            else
                return Self.GetOrientationFast(tf);
        }

        public Vector3 GetTangent(float tf)
        {
            if (UseWorldCoordinates)
                return Self.transform.localToWorldMatrix.MultiplyVector(Self.GetTangent(tf));
            else
                return Self.GetTangent(tf);
        }

        public Vector3 GetTangentFast(float tf)
        {
            if (UseWorldCoordinates)
                return Self.transform.localToWorldMatrix.MultiplyVector(Self.GetTangentFast(tf));
            else
                return Self.GetTangentFast(tf);
        }

        public Vector3 Interpolate(float tf)
        {
            if (UseWorldCoordinates)
                return Self.transform.localToWorldMatrix.MultiplyPoint(Self.Interpolate(tf));
            else
                return Self.Interpolate(tf);
        }

        public Vector3 InterpolateFast(float tf)
        {
            if (UseWorldCoordinates)
                return Self.transform.localToWorldMatrix.MultiplyPoint(Self.InterpolateFast(tf));
            else
                return Self.InterpolateFast(tf);
        }

        public void Refresh()
        {
            Self.Refresh();
        }

        public void CreateSplineSegmentAdapters()
        {
            if (Self != null)
            {
                var segments = Self.GetComponentsInChildren<CurvySplineSegment>();
                foreach (var segment in segments)
                {
                    if (segment.gameObject.GetComponent<CurvySplineSegmentAdapter>() == null)
                    {
                        var adapter = segment.gameObject.AddComponent<CurvySplineSegmentAdapter>();
                    }
                }
            }
        }

        [ContextMenu("Configure")]
        public void Configure()
        {
            CreateSplineSegmentAdapters();
        }

        public void OnBeforeSerialize()
        {
            Self = Self ?? GetComponent<CurvySpline>();
            // Creating components needs to be done outside of ISerializationCallbackReceiver methods
            EditorUtilities.AddEditorDelayedOneShotCall(CreateSplineSegmentAdapters);
        }

        public void OnAfterDeserialize()
        {
        }
    }
}

#endif
