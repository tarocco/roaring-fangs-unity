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

using Midi;
using Midi.Chunks;
using System.Collections.Generic;

namespace RoaringFangs.Formats.MIDI
{
    public class MIDIEventList : MIDIHelperBase
    {
        private List<MIDIEventHelper> _Events;

        public int NumberOfEvents
        {
            get { return _Events.Count; }
        }

        protected override List<MIDIEventHelper> pEvents
        {
            get
            {
                return _Events;
            }
            set
            {
                _Events = value;
            }
        }

        private string _Name;

        public string Name
        {
            get { return _Name; }
            private set { _Name = value; }
        }

        protected override string pName
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }

        public bool GetEvent(int index, out MIDIEventHelper @event)
        {
            bool result = index >= 0 && index < _Events.Count;
            if (result)
                @event = _Events[index];
            else
                @event = null;
            return result;
        }

        private IEnumerable<int> GetIndicesAfter(float beats)
        {
            // Index immediately after time
            int start = FirstIndexAfterTime(beats);
            for (int i = start; i < _Events.Count; i++)
            {
                yield return i;
            }
        }

        /// <summary>
        /// Get all the notes after a given time
        /// </summary>
        /// <param name="beats">Time in beats notes, NOT seconds!</param>
        /// <returns></returns>
        public IEnumerable<MIDIEventHelper> GetEventsAfter(float beats)
        {
            foreach (int i in GetIndicesAfter(beats))
                yield return _Events[i];
        }

        public IEnumerable<MIDIEventHelper> GetEventsAfter(int index)
        {
            for (; index < _Events.Count; index++)
                yield return _Events[index];
        }

        private int FirstIndexAfterTime(float beats)
        {
            return FirstIndexAfterTime(beats, _Events.Count / 2);
        }

        private int FirstIndexAfterTime(float beats, int current_index)
        {
            return _FirstIndexAfterTime(beats, current_index, 2);
        }

        private int _FirstIndexAfterTime(float beats, int current_index, int depth)
        {
            // Edge cases
            if (current_index <= 0)
                return 0;
            int last = _Events.Count - 1;
            if (current_index >= last)
                return last;

            // Essentially a modified binary search
            var previous = _Events[current_index - 1];
            var here = _Events[current_index];
            if (previous.TimeBeats < beats && here.TimeBeats >= beats)
                return current_index;

            int slide = _Events.Count >> depth;
            if (here.TimeBeats > beats)
                slide = -slide;
            return _FirstIndexAfterTime(beats, current_index + slide, depth + 1);
        }

        public IEnumerable<MIDIEventHelper> GetEventsUntil(int start_index, float beats)
        {
            MIDIEventHelper @event;
            while (GetEvent(start_index++, out @event) && @event.TimeBeats < beats)
                yield return @event;
        }

        public MIDIEventList(MidiData midi_data, TrackChunk track, MIDIEventFilter filter)
        {
            // Calculate the time scale as the reciprocal of the MIDI time_division
            // time_division is also known as PPQ https://en.wikipedia.org/wiki/Pulses_per_quarter_note
            float time_scale = 1f / (float)(midi_data.header.time_division);
            // Instatiate the event list
            pEvents = new List<MIDIEventHelper>();
            // Starting from time 0.0
            float time = 0.0f;
            // Keep track of the last note-on events and match them with note-off events that follow,
            // then store the time between the events as the duration in the event helper
            MIDIEventHelper[] notes_on_last = new MIDIEventHelper[256];
            // Keep track of the valid event index
            int index = 0;
            // Add all of the events needed for this track to the backing list (pEvents)
            foreach (var enum_midi_event in track.events)
            {
                // Calculate the event time based on delta_time
                // delta_time is measured in ticks
                time += time_scale * (float)enum_midi_event.delta_time;
                if (filter.Accepts(enum_midi_event))
                {
                    // Create an event helper (wrapper for event and details)
                    MIDIEventHelper current_event_helper =
                        new MIDIEventHelper(enum_midi_event, (int)index, time);
                    // Add the event helper to the backing list
                    pEvents.Add(current_event_helper);
                    // If this event is a note off event
                    if (enum_midi_event is Midi.Events.ChannelEvents.NoteOffEvent)
                    {
                        var note_off = enum_midi_event as Midi.Events.ChannelEvents.NoteOffEvent;
                        int number = note_off.note_number;
                        var on_event_helper = notes_on_last[number];
                        if (on_event_helper != null)
                        {
                            float duration = time - on_event_helper.TimeBeats;
                            on_event_helper.DurationBeats = duration;
                            current_event_helper.DurationBeats = duration;
                        }
                        // The note off event is not accounted for when consolidating
                    }
                    //  If this event is a note on event
                    else if (enum_midi_event is Midi.Events.ChannelEvents.NoteOnEvent)
                    {
                        // Store it as the last note-on event for this note number
                        var note_on = enum_midi_event as Midi.Events.ChannelEvents.NoteOnEvent;
                        // Quantize the time to 16th notes
                        current_event_helper.TimeBeats = Quantization(current_event_helper.TimeBeats, 16);
                        int number = note_on.note_number;
                        notes_on_last[number] = current_event_helper;
                    }
                    index++;
                }
                else if (MIDIEventFilter.TrackNameFilter.Accepts(enum_midi_event))
                {
                    Name = track.chunk_ID ?? "";

                    // TODO: get this working
                    //var name = (m_e.Element as Midi.Events.MetaEvents.SequenceOrTrackNameEvent).name;
                    // Small workaround for bug in MIDI parsing library
                    //Name = name.Trim(new[] {'\0'});
                }
            }
        }

        //Quick Quantization function without rounding function
        //Basically "round 'n' to the nearest increment of '1/b'"
        private float Quantization(float n, int b)
        {
            float q = n * b;
            float r = q % 1;
            float s = (r * 2) - (r * 2) % 1;
            return (q - r + s) / b;
        }
    }
}