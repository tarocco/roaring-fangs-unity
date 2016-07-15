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

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Midi;
using Midi.Chunks;
using Midi.Events;

using RoaringFangs.Utility;

namespace RoaringFangs.Formats.MIDI
{
    public class MIDIHelper : MIDIHelperBase
    {
        private List<MIDIEventList> _EventLists;
        public int NumberOfEventLists
        {
            get { return _EventLists.Count; }
        }
        public MIDIEventList GetEventListByIndex(int index)
        {
            return _EventLists[index];
        }
        public MIDIEventList GetEventListByName(string name)
        {
            return _EventLists.First((list) =>
            {
                return list.Name == name;
            });
        }
        /*
        private Midi.MidiData _MIDIData;
        private Midi.MidiData MIDIData
        {
            get { return _MIDIData; }
            set { _MIDIData = value; }
        }
        */
        public void Load(Midi.MidiData data)
        {
            //MIDIData = data;
            _EventLists = new List<MIDIEventList>();
            // For every track in the MIDI data
            // Skip the first track as it contains only meta events
            foreach (TrackChunk track in data.tracks)
            {
                // Create a new event list of note on/off events for this MIDI track
                MIDIEventList list = new MIDIEventList(data, track, MIDIEventFilter.NotesOnOffFilter);
                // Add the backing list to the EventLists list.
                // These should be added in the same order as the tracks
                // are read so that they correspond.
                _EventLists.Add(list);
            }
        }
        public MIDIHelper()
        {

        }
    }
}