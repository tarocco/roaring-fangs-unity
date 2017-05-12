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

namespace RoaringFangs.Motion
{
    // Using InControl seems reaaallly overkill for this...
    //public class MouseActionSet : PlayerActionSet
    //{
    //    public PlayerAction Up, Down, Left, Right;
    //    public PlayerTwoAxisAction MouseAction;

    //    public MouseActionSet()
    //    {
    //        Up = CreatePlayerAction("Up");
    //        Down = CreatePlayerAction("Down");
    //        Left = CreatePlayerAction("Left");
    //        Right = CreatePlayerAction("Right");
    //        MouseAction = CreateTwoAxisPlayerAction(Left, Right, Down, Up);
    //    }
    //}
    public class MouseFollower : MonoBehaviour
    {
        public float RayDistance = 100f;

        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            var camera = Camera.main;
            if (camera != null)
            {
                var xy = new Plane(Vector3.back, Vector3.zero);
                var screen_position = Input.mousePosition;
                screen_position.z = RayDistance;
                var ray = camera.ScreenPointToRay(screen_position);
                float t;
                xy.Raycast(ray, out t);
                var position = ray.origin + ray.direction*t;
                transform.position = position;
            }
        }
    }
}