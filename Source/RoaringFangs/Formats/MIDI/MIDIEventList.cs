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
    public class MIDIEventList : MIDIHelperBase
    {
        private List<MIDIEventHelper> _Events;
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
            MIDIEventHelper[] last_helper_per_note_number = null;
            last_helper_per_note_number = new MIDIEventHelper[256];
            // Count and index each of the elements that get added to pEvents
            int index = 0;
            // Add all of the events for this track to the backing list (pEvents)
            foreach (var m_e_h in track.events.Enumerate())
            {
                // Calculate the event time based on delta_time
                // delta_time is measured in ticks
                time += time_scale * (float)m_e_h.Element.delta_time;
                if(filter.Accepts(m_e_h.Element))
                {
                    
                    // Create an event helper (wrapper for event and details)
                    MIDIEventHelper event_helper =
                        new MIDIEventHelper(m_e_h.Element, (int)m_e_h.Index, time);
                    // Add the event helper to the backing list
                    pEvents.Add(event_helper);

                    // If this event is a note off event
                    if (m_e_h.Element is Midi.Events.ChannelEvents.NoteOffEvent)
                    {
                        var note_off = m_e_h.Element as Midi.Events.ChannelEvents.NoteOffEvent;
                        var corresponding_helper = last_helper_per_note_number[note_off.note_number];
                        if (corresponding_helper != null)
                        {
                            float duration = time - corresponding_helper.TimeBeats;
                            corresponding_helper.DurationBeats = duration;
                            event_helper.DurationBeats = duration;
                        }
                        // The note off event is not accounted for when consolidating
                    }

                    //  If this event is a note on event
                    if (m_e_h.Element is Midi.Events.ChannelEvents.NoteOnEvent)
                    {
                        // Store it as the last note-on event for this note number
                        var note_on = m_e_h.Element as Midi.Events.ChannelEvents.NoteOnEvent;
                        last_helper_per_note_number[note_on.note_number] = event_helper;
                    }
                }
                if(MIDIEventFilter.TrackNameFilter.Accepts(m_e_h.Element))
                {
                    // NOTE: Unfortunately I can't seem to get the track name working yet,
                    // so we're going to make up a name instead. It's just the index as a string.

                    Name = m_e_h.Index.ToString();
                        
                    // TODO: get this working
                    //var name = (m_e.Element as Midi.Events.MetaEvents.SequenceOrTrackNameEvent).name;
                    // Small workaround for bug in MIDI parsing library
                    //Name = name.Trim(new[] {'\0'});
                }
            }
        }
    }
}