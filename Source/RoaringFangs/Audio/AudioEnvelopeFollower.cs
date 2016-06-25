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
using System.Collections;
using System.Linq;

using RoaringFangs.Attributes;

namespace RoaringFangs.Audio
{
    public class AudioEnvelopeFollower : MonoBehaviour
    {
        [SerializeField]
        [AutoProperty(Delayed = true)]
        private int _ChannelBufferSize = 8192;
        public int ChannelBufferSize
        {
            get { return _ChannelBufferSize; }
            set { _ChannelBufferSize = value; }
        }

        [SerializeField]
        [AutoProperty(Delayed = true)]
        private int _ChannelNumber = 0;
        public int ChannelNumber
        {
            get { return _ChannelNumber; }
            set
            {
                if (_ChannelBuffer != null && _ChannelBuffer.Length != value)
                    _ChannelBuffer = new float[value];
                _ChannelNumber = value;
            }
        }

        [SerializeField]
        private AnimationCurve _IntegrationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public AnimationCurve IntegrationCurve
        {
            get { return _IntegrationCurve; }
            set { _IntegrationCurve = value; }
        }

        [SerializeField]
        [AutoProperty]
        private float _Gain = 1f;
        public float Gain
        {
            get { return _Gain; }
            private set { _Gain = value; }
        }

        
        private float _Level;
        public float Level
        {
            get { return _Level; }
            private set { _Level = value; }
        }

        private float[] _ChannelBuffer;
        private float[] ChannelBuffer
        {
            get
            {
                if (_ChannelBuffer == null || _ChannelBuffer.Length != _ChannelBufferSize)
                    _ChannelBuffer = new float[_ChannelBufferSize];
                return _ChannelBuffer;
            }
        }

        private float Rectify(float x)
        {
            return Mathf.Abs(x);
        }

        void Start()
        {

        }

        void Update()
        {
            AudioListener.GetOutputData(ChannelBuffer, _ChannelNumber);
            float integrator_duration = _IntegrationCurve.keys.Last().time;
            float evaluation_coefficient = 1f / (integrator_duration * ChannelBufferSize);
            float integrator_sum = 0f;
            float integrated_level = 0f;
            for (int i = 0; i < ChannelBufferSize; i++)
            {
                float t = evaluation_coefficient * i;
                float f = IntegrationCurve.Evaluate(t);
                float sample = ChannelBuffer[i];
                float sample_rectified = Rectify(ChannelBuffer[i]);
                integrator_sum += f;
                integrated_level += f * sample_rectified;
            }
            Level = Gain * integrated_level / integrator_sum;
        }
    }
}