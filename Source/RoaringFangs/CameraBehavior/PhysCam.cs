/*
The MIT License (MIT)

Copyright (c) 2016 Roaring Fangs Entertainment

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using RoaringFangs.Adapters.FluffyUnderware.Curvy;
using System.Linq;
using RoaringFangs.Attributes;
using RoaringFangs.Editor;
using UnityEngine;

namespace RoaringFangs.CameraBehavior
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysCam : MonoBehaviour, ISerializationCallbackReceiver
    {
        public GameObject Target;

        [SerializeField, AutoProperty("GuideCameraDirection")]
        private MonoBehaviour _GuideCameraDirectionBehavior;

        public ICurvySpline GuideCameraDirection
        {
            get { return _GuideCameraDirectionBehavior as ICurvySpline; }
            set { _GuideCameraDirectionBehavior = value as MonoBehaviour; }
        }

        public GameObject CameraParent;
        public Camera Camera;

        public LayerMask CameraPusherMask;
        public Collider Detector;
        public bool RollUp = false;
        private Rigidbody Rigidbody;

        private void Start()
        {
            Rigidbody = GetComponent<Rigidbody>();
            //transform.position = Target.transform.position;
        }

        private void FixedUpdate()
        {
            var colliders = Physics.OverlapBox(Detector.bounds.center, Detector.bounds.extents, transform.rotation, CameraPusherMask);
            bool any_others = colliders.Except(new[] { Detector }).Any();
            if (!any_others)
                transform.position = Target.transform.position;

            if (RollUp)
            {
                Quaternion rotation = Rigidbody.rotation;
                rotation = Quaternion.LookRotation(rotation * Vector3.forward, Vector3.up);
                Rigidbody.MoveRotation(rotation);
            }
        }

        private void LateUpdate()
        {
            if (GuideCameraDirection != null)
            {
                float tf_nearest = GuideCameraDirection.GetNearestPointTF(transform.position);
                Vector3 tangent = GuideCameraDirection.GetTangent(tf_nearest);
                Vector3 normal = Vector3.Cross(tangent, Vector3.up);
                Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);
                CameraParent.transform.localRotation = rotation;
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