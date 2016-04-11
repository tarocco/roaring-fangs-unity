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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Midi;

namespace RoaringFangs.Formats
{
    /// <summary>
    /// A not-so-efficient but simple-as-heck way of representing a MIDI track
    /// </summary>
    public class FlatMIDITrack
    {
        private float _BeatStep;
        private float _BeatRate;
        public float BeatStep
        {
            get { return _BeatStep; }
            private set
            {
                _BeatStep = value;
                _BeatRate = 1f / value;
            }
        }
        public float BeatRate
        {
            get { return _BeatRate; }
            private set
            {
                _BeatRate = value;
                _BeatStep = 1f / value;
            }
        }

        private int _NoteCount = 0;
        public int NoteCount
        {
            get { return _NoteCount; }
            private set { _NoteCount = value; }
        }

        public readonly Midi.Events.MidiEvent[][] Buffer;
        public FlatMIDITrack(Midi.MidiData data, Midi.Chunks.TrackChunk track, int beats_per_measure = 4)
        {
            BeatRate = data.header.time_division;
            int time_counter = 0;
            foreach (Midi.Events.MidiEvent e in track.events)
                time_counter += e.delta_time;
            Buffer = new Midi.Events.MidiEvent[time_counter][];
            time_counter = 0;
            var events = new List<Midi.Events.MidiEvent>();
            foreach (Midi.Events.MidiEvent e in track.events)
            {
                var note = e as Midi.Events.ChannelEvents.NoteOnEvent;
                bool dt_p = e.delta_time > 0;
                if (dt_p)
                {
                    if (events != null)
                        Buffer[time_counter] = events.ToArray();
                    events = new List<Midi.Events.MidiEvent>();
                }
                if (note != null)
                {
                    events.Add(note);
                    _NoteCount++;
                }
                time_counter += e.delta_time;
            }
        }
        public static FlatMIDITrack Read(Stream midi_file_stream)
        {
            Profiler.BeginSample("MIDI File Parsing");
            var midi_data = Midi.FileParser.parse(midi_file_stream);
            Profiler.EndSample();
            Profiler.BeginSample("MIDI track conversion");
            foreach (Midi.Chunks.TrackChunk track in midi_data.tracks)
            {
                var flat = new FlatMIDITrack(midi_data, track);
                // Sanity check: ignore "empty" tracks
                // TODO: keep this or not???
                if (flat.NoteCount > 0)
                    return flat;
            }
            Profiler.EndSample();
            return null;
        }
    }
}