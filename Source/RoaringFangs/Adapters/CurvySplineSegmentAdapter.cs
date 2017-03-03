#if FLUFFYUNDERWARE_CURVY

using System;
using System.Collections.Generic;
using RoaringFangs.Adapters.FluffyUnderware.Curvy;
using RoaringFangs.Attributes;
using RoaringFangs.Utility;
using UnityEngine;
using FluffyUnderware.Curvy;

namespace RoaringFangs.Adapters
{
    [RequireComponent(typeof(CurvySplineSegment))]
    public class CurvySplineSegmentAdapter : MonoBehaviour, ICurvySplineSegment, ISerializationCallbackReceiver
    {
        [SerializeField]
        private CurvySplineSegment _Self;

        public CurvySplineSegment Self
        {
            get { return _Self; }
            set { _Self = value; }
        }
        //new public Transform transform
        //{
        //    get
        //    {
        //        return Self.transform;
        //    }
        //}

        public void OnBeforeSerialize()
        {
            Self = Self ?? GetComponent<CurvySplineSegment>();
        }

        public void OnAfterDeserialize()
        {
        }
    }
}

#endif
