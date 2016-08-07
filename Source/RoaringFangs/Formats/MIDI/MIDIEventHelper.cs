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

using Midi.Events;
using System;
using UnityEngine;

namespace RoaringFangs.Formats.MIDI
{
    [Serializable]
    public class MIDIEventHelper
    {
        [SerializeField]
        private MidiEvent _Event;

        /// <summary>
        /// The MidiEvent being helped
        /// </summary>
        public MidiEvent MIDIEvent
        {
            get { return _Event; }
            set { _Event = value; }
        }

        [SerializeField]
        private float _TimeBeats;

        /// <summary>
        /// Time at which the event takes place, measured in quarter notes, NOT seconds
        /// </summary>
        public float TimeBeats
        {
            get { return _TimeBeats; }
            set { _TimeBeats = value; }
        }

        [SerializeField]
        private float _TimeMeasures;
        public float TimeMeasures
        {
            get { return _TimeMeasures; }
            set { _TimeMeasures = value; }
        }


        [SerializeField]
        private float _DurationBeats = 0;

        /// <summary>
        /// Duration of the event measured in quarter notes, NOT seconds
        /// </summary>
        public float DurationBeats
        {
            get { return _DurationBeats; }
            set { _DurationBeats = value; }
        }

        [SerializeField]
        private int _Index;

        /// <summary>
        /// Event index as counted when read from MIDI data chunks
        /// </summary>
        public int Index
        {
            get { return _Index; }
            set { _Index = value; }
        }

        public static bool IsInSameMeasure(MIDIEventHelper a, MIDIEventHelper b)
        {
            int measure_a = Mathf.FloorToInt(a.TimeMeasures);
            int measure_b = Mathf.FloorToInt(b.TimeMeasures);
            return measure_a == measure_b;
        }

        public override string ToString()
        {
            return "MIDIEvent: " + MIDIEvent.ToString() + " - TimeBeats: " + TimeBeats + " - DurationBeats: " + DurationBeats;
        }
    }
}