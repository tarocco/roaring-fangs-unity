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

#if UNITY_TEST
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityTest;

using RoaringFangs.Formats.MIDI;
using RoaringFangs.Utility;

namespace RoaringFangs.Formats.Tests
{
    [IntegrationTest.DynamicTest("Rhythm Game Tests")]
    class Test_MidiHelper : MonoBehaviour
    {
        public const string PathTests = "Tests";
        public const string DirectoryMIDI = "MIDI";
        private MIDIHelper MIDIHelper;
        void Start()
        {
            var www_midi = new WWW(Assets.StreamingAssetsPathCombine(PathTests, DirectoryMIDI, "Test1.mid"));
            using (var midi_file_stream = new MemoryStream(www_midi.bytes))
            {
                var midi_data = Midi.FileParser.parse(midi_file_stream);
                // Test for 9 note-on events
                int track0_on_events_count = midi_data.tracks.First().events.Count((m_e) => { return m_e is Midi.Events.ChannelEvents.NoteOnEvent; });
                IntegrationTest.Assert(track0_on_events_count == 9, "Number of events should be 9, got " + track0_on_events_count);
                // Load the MIDI data into a MIDI helper
                var MIDIHelper = new MIDIHelper();
                MIDIHelper.Load(midi_data);
                // Get the first event list from the MIDI helper
                var event_list0 = MIDIHelper.GetEventListByIndex(0);
                Debug.Log("Track name is \"" + event_list0.Name + "\"");
                // Assert that we can find the note-on event at 2.0 beats (quarter notes)
                var next_notes = event_list0.GetEventsAfter(1.9f);
                float note_time = next_notes.First().TimeBeats;
                IntegrationTest.Assert(note_time == 2.0f, "Note time should be 2.0 beats, got " + note_time);
            }
            IntegrationTest.Pass();
        }
        public void FirstIndexAfterTime_Test1()
        {
        }
    }
}
#endif