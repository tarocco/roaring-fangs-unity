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
    //TODO: FIX THIS
    public class MouseFollower : MonoBehaviour
    {
        //public Receiver Receiver;
        private float RayDistance = 100f;

        private void Start()
        {
            //Receiver.MultiMousePress += HandleMultiMousePress;
        }

        private void OnDestroy()
        {
            //Receiver.MultiMousePress -= HandleMultiMousePress;
        }

        /*
        void HandleMultiMousePress(object sender, MultiMousePressEventArgs m_args)
        {
            foreach (MousePressEventArgs args in m_args.MousePressed)
            {
                Plane xy = new Plane(Vector3.back, Vector3.zero);
                Vector3 screen_position = args.ScreenPosition;
                screen_position.z = RayDistance;
                Ray ray = Camera.main.ScreenPointToRay(screen_position);
                float t;
                xy.Raycast(ray, out t);
                Vector3 position = ray.origin + ray.direction * t;
                transform.position = position;
                break;
            }
        }
        */

        // Update is called once per frame
        private void Update()
        {
        }
    }
}