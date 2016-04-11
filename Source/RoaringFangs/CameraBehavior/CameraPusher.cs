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

using UnityEngine;
using System;
using System.Collections;

namespace RoaringFangs.CameraBehavior
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Rigidbody2D))]
    public class CameraPusher : MonoBehaviour
    {
        private Rigidbody2D Self;
        public Rigidbody2D Target;

        public BoxCollider2D MainBox;

        public Bounds DeadZoneInner;
        public Bounds DeadZoneOuter;

        public Vector2 GlobalBoundsLo = new Vector2(float.NegativeInfinity, 0f);
        public Vector2 GlobalBoundsHi = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

        public float SpeedScale = 1.0f;
        public float SpeedSmooth = 1.0f;

        private Vector2 TargetVelocity;
        private Vector2 TargetVelocity_Abs;

        void Start()
        {
            Self = GetComponent<Rigidbody2D>();
        }
        void Update()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                UpdateColliders();
            }
#endif
        }
        void FixedUpdate()
        {
            TargetVelocity = Vector2.Lerp(TargetVelocity, Target.velocity, SpeedSmooth * Time.fixedDeltaTime);
            Vector2 target_velocity_abs = new Vector2(Mathf.Abs(Target.velocity.x), Mathf.Abs(Target.velocity.y));
            TargetVelocity_Abs = Vector2.Lerp(TargetVelocity_Abs, target_velocity_abs, SpeedSmooth * Time.fixedDeltaTime);

            Vector2 scale = new Vector2(
                SpeedScale * TargetVelocity_Abs.x,
                SpeedScale * TargetVelocity_Abs.y);

            Vector2 target_position;
            Vector2 main_box_extents = 0.5f * MainBox.size;
            target_position.x = Mathf.Clamp(
                Target.position.x + MainBox.offset.x,
                GlobalBoundsLo.x + main_box_extents.x,
                GlobalBoundsHi.x - main_box_extents.x);
            target_position.y = Mathf.Clamp(
                Target.position.y + MainBox.offset.y,
                GlobalBoundsLo.y + main_box_extents.y,
                GlobalBoundsHi.y - main_box_extents.y);

            Self.MovePosition(target_position);
            UpdateColliders(scale);
        }
        void UpdateColliders(Vector2 scale = default(Vector2))
        {
            float aspect_ratio = Camera.main.aspect;
            Vector2 size = RoaringFangs.Utility.Math2.LerpV2(DeadZoneInner.size, DeadZoneOuter.size, scale);
            size.x = size.x / aspect_ratio;
            Vector2 offset = RoaringFangs.Utility.Math2.LerpV2(DeadZoneInner.center, DeadZoneOuter.center, scale);
            offset.x = offset.x / aspect_ratio;
            MainBox.size = size;
            MainBox.offset = offset;
        }
        private Bounds AspectBounds2D(Bounds bounds)
        {
            float aspect = Camera.main.aspect;
            return new Bounds(bounds.center, new Vector3(aspect * bounds.size.x, bounds.size.y));
        }
    }
}