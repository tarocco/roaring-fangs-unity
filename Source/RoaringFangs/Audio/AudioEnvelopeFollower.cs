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
using System;

namespace RoaringFangs.Audio
{
    public class AudioEnvelopeFollower : MonoBehaviour
    {
        #region Types
        public enum EFollowerMode
        {
            Maximum,
            Integrate,
        }

        public enum EStrideMode
        {
            Skip,
            Maximum,
            Average,
        }

        public delegate float Rectifier(float value);
        public delegate void Collector(ref float basis, float value);
        #endregion
        #region Properties
        [SerializeField]
        [AutoProperty]
        private AudioSource _AudioSource;
        public AudioSource AudioSource
        {
            get { return _AudioSource; }
            set { _AudioSource = value; }
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
        [AutoProperty]
        private float _InputGain = 1f;
        public float InputGain
        {
            get { return _InputGain; }
            private set { _InputGain = value; }
        }

        [SerializeField]
        [AutoProperty]
        private float _OutputGain = 1f;
        public float OutputGain
        {
            get { return _OutputGain; }
            private set { _OutputGain = value; }
        }

        [SerializeField]
        [AutoProperty]
        private EFollowerMode _FollowerMode = EFollowerMode.Maximum;
        public EFollowerMode FollowerMode
        {
            get { return _FollowerMode; }
            set { _FollowerMode = value; }
        }

        [SerializeField]
        [AutoProperty]
        private EStrideMode _StrideMode = EStrideMode.Maximum;
        public EStrideMode StrideMode
        {
            get { return _StrideMode; }
            set { _StrideMode = value; }
        }

        [SerializeField]
        private AnimationCurve _IntegrationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public AnimationCurve IntegrationCurve
        {
            get { return _IntegrationCurve; }
            set { _IntegrationCurve = value; }
        }

        [SerializeField]
        [AutoProperty(Delayed = true)]
        private int _BufferStride = 24;
        public int BufferStride
        {
            get { return Mathf.Max(1, _BufferStride); }
            set { _BufferStride = Mathf.Max(1, value); }
        }

        [SerializeField]
        [AutoProperty(Delayed = true)]
        private int _IncrementalBufferSize = 1024;
        public int IncrementalBufferSize
        {
            get { return _IncrementalBufferSize; }
            set { _IncrementalBufferSize = value; }
        }

        [SerializeField]
        [AutoProperty(Delayed = true)]
        private int _ChannelBufferSize = 1024;
        public int ChannelBufferSize
        {
            get { return _ChannelBufferSize; }
            set { _ChannelBufferSize = Mathf.Clamp(value, 0, 16384); }
        }

        protected float[] _ChannelBuffer;
        public float[] ChannelBuffer
        {
            get
            {
                if (_ChannelBuffer == null || _ChannelBuffer.Length != _ChannelBufferSize)
                {
                    _ChannelBuffer = new float[_ChannelBufferSize];
                }
                return _ChannelBuffer;
            }
        }

        private int _IncrementalBufferHead = 0;
        protected int IncrementalBufferHead
        {
            get { return _IncrementalBufferHead; }
            set { _IncrementalBufferHead = value; }
        }

        protected float[] _IncrementalBuffer;
        public float[] IncrementalBuffer
        {
            get
            {
                if (_IncrementalBuffer == null || _IncrementalBuffer.Length != _IncrementalBufferSize)
                {
                    _IncrementalBuffer = new float[_IncrementalBufferSize];
                    _IncrementalBufferHead = 0;
                }
                return _IncrementalBuffer;
            }
        }

        private float _Level;
        public float Level
        {
            get { return _Level; }
            private set { _Level = value; }
        }
        #endregion
        #region Methods
        #region Static
        protected static void Buffer(
            AudioSource audio_source,
            ref int audio_source__ts__prev,
            float[] channel_buffer,
            int channel_number,
            float[] incremental_buffer,
            ref int buffer_read_head,
            out int buffer_read_start_index,
            int buffer_stride,
            Collector stride_collector,
            Collector stride_divisor_collector)
        {
            int sample_count;
            if (audio_source)
            {
                // Get channel data from audio source
                sample_count = audio_source.timeSamples - audio_source__ts__prev;
                audio_source__ts__prev = audio_source.timeSamples;
                audio_source.GetOutputData(channel_buffer, channel_number);
            }
            else
            {
                // Get channel data from audio listener
                sample_count = Mathf.FloorToInt(AudioSettings.outputSampleRate * Time.deltaTime);
                AudioListener.GetOutputData(channel_buffer, channel_number);
            }
            int offset_read = Mathf.Max(0, channel_buffer.Length - sample_count);
            var i_write = buffer_read_head;
            buffer_stride = Mathf.Max(1, buffer_stride);
            if (stride_collector != null)
            {
                if (stride_divisor_collector != null)
                {
                    for (int i = offset_read; i < channel_buffer.Length; i += buffer_stride)
                    {
                        i_write = (i_write + 1) % incremental_buffer.Length;
                        float collected_sample = 0f;
                        float collected_divisor = 0f;
                        int end = Mathf.Min(i + buffer_stride, channel_buffer.Length);
                        for (int i_read = i; i_read < end; i_read++)
                        {
                            stride_collector(ref collected_sample, channel_buffer[i_read]);
                            stride_divisor_collector(ref collected_divisor, 1f);
                        }
                        incremental_buffer[i_write] = collected_sample / collected_divisor;
                    }
                }
                else
                {
                    for (int i = offset_read; i < channel_buffer.Length; i += buffer_stride)
                    {
                        i_write = (i_write + 1) % incremental_buffer.Length;
                        float collected_sample = 0f;
                        int end = Mathf.Min(i + buffer_stride, channel_buffer.Length);
                        for (int i_read = i; i_read < end; i_read++)
                            stride_collector(ref collected_sample, channel_buffer[i_read]);
                        incremental_buffer[i_write] = collected_sample;
                    }
                }
            }
            else
            {
                for (int i = offset_read; i < channel_buffer.Length; i += buffer_stride)
                {
                    i_write = (i_write + 1) % incremental_buffer.Length;
                    incremental_buffer[i_write] = channel_buffer[i];
                }
            }
            buffer_read_start_index = buffer_read_head;
            buffer_read_head = i_write;
        }
        protected static float GetRawLevel(
            float[] buffer,
            int samples_to_read,
            int index_start_offset,
            AnimationCurve integration_curve,
            float input_gain,
            Rectifier rectify,
            Collector collector,
            Collector divisor_collector = null)
        {
            float integrator_duration = integration_curve.keys.Last().time;
            float evaluation_coefficient = 1f / integrator_duration;
            float integrated_divisor = divisor_collector != null ? 0f : 1f;
            float integrated_level = 0f;
            float samples_to_read_f = (float)samples_to_read;
            for (int i = 0; i < samples_to_read; i++)
            {
                int read_index = (index_start_offset + i) % buffer.Length;
                float t = evaluation_coefficient * i / samples_to_read_f;
                float f = integration_curve.Evaluate(t);
                float sample = buffer[read_index];
                float sample_rectified = rectify(sample);
                if (divisor_collector != null)
                    divisor_collector(ref integrated_divisor, f);
                collector(ref integrated_level, input_gain * f * sample_rectified);
            }
            return integrated_level / integrated_divisor;
        }
        #endregion

        private int AudioSource__timeSamples__previous; // c-static
        protected void Buffer(
            out int buffer_read_start_index,
            Collector stride_collector,
            Collector stride_divisor_collector)
        {
            int incremental_buffer_head = IncrementalBufferHead;
            Buffer(
                AudioSource,
                ref AudioSource__timeSamples__previous,
                ChannelBuffer,
                ChannelNumber,
                IncrementalBuffer,
                ref incremental_buffer_head,
                out buffer_read_start_index,
                BufferStride,
                stride_collector,
                stride_divisor_collector);
            IncrementalBufferHead = incremental_buffer_head;
        }
        protected void Buffer(out int buffer_read_start_index)
        {
            switch (StrideMode)
            {
                default:
                case EStrideMode.Skip:
                    Buffer(out buffer_read_start_index, null, null);
                    break;
                case EStrideMode.Maximum:
                    Buffer(out buffer_read_start_index, CollectMax, null);
                    break;
                case EStrideMode.Average:
                    Buffer(out buffer_read_start_index, CollectSum, CollectSum);
                    break;
            }
        }
        protected float GetRawLevel(
            int index_start_offset,
            Collector collector,
            Collector divisor_collector)
        {
            return GetRawLevel(
                IncrementalBuffer,
                IncrementalBufferSize,
                index_start_offset,
                IntegrationCurve,
                InputGain,
                Rectify,
                collector,
                divisor_collector);
        }
        protected float GetRawLevel(int buffer_read_start_index)
        {
            switch (FollowerMode)
            {
                default:
                case EFollowerMode.Maximum:
                    return GetRawLevel(buffer_read_start_index, CollectMax, null);
                case EFollowerMode.Integrate:
                    return GetRawLevel(buffer_read_start_index, CollectSum, CollectSum);
            }
        }

        protected float Rectify(float value)
        {
            return Mathf.Abs(value);
        }

        protected void CollectMax(ref float max, float amount)
        {
            max = Mathf.Max(max, amount);
        }

        protected void CollectSum(ref float sum, float amount)
        {
            sum = sum + amount;
        }

        void Start()
        {

        }

        void Update()
        {
            int buffer_read_start_index;
            Buffer(out buffer_read_start_index);
            Level = OutputGain * GetRawLevel(buffer_read_start_index);
        }
        #endregion
    }
}