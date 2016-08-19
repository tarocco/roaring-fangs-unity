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

using Midi.Chunks;
using Midi.Events;
using Midi.Events.ChannelEvents;
using System.Collections.Generic;
using System.Linq;

namespace RoaringFangs.Formats.MIDI
{
    public static class MIDIHelper
    {
        public static IEnumerable<T> ReadNoteEventInfo<T>(MidiEvent[] events, float ppq)
            where T : INoteEventInfo, new()
        {
            float beat_counter = 0f;
            float measure_counter = 0f;
            for(int i = 0; i < events.Length; i++)
            {
                var @event = events[i];
                beat_counter += @event.delta_time / ppq;
                measure_counter = beat_counter / 4f; // TODO
                if (@event is NoteOnEvent)
                {
                    NoteOnEvent on_event;
                    NoteOffEvent off_event;
                    int time_span_in_pulses;
                    bool match = FindNoteOnOff(
                        events,
                        i,
                        out on_event,
                        out off_event,
                        out time_span_in_pulses,
                        32);
                    if (match)
                    {
                        int note_number = on_event.note_number;
                        float duration_beats = time_span_in_pulses / ppq;
                        var result = new T()
                        {
                            TimeBeats = beat_counter,
                            TimeMeasures = measure_counter,
                            DurationBeats = duration_beats,
                            NoteNumber = note_number,
                        };
                        yield return result;
                    }
                }
            }
        }

        /// <summary>
        /// Finds the first pair of same-note on and off events
        /// in an array of MIDI events starting from <paramref name="start_index"/>
        /// </summary>
        /// <param name="events"></param>
        /// <param name="start_index"></param>
        /// <param name="on_event"></param>
        /// <param name="off_event"></param>
        /// <param name="time_span_in_pulses"></param>
        /// <param name="max_steps"></param>
        /// <returns></returns>
        public static bool FindNoteOnOff(
            MidiEvent[] events,
            int start_index,
            out NoteOnEvent on_event,
            out NoteOffEvent off_event,
            out int time_span_in_pulses,
            int max_steps)
        {
            on_event = null;
            off_event = null;
            time_span_in_pulses = 0;
            for(int i = 0; i < max_steps; i++)
            {
                int index = start_index + i;
                MidiEvent @event = events[index];
                if (on_event != null)
                {
                    time_span_in_pulses += @event.delta_time;
                    if (@event is NoteOffEvent)
                    {
                        off_event = @event as NoteOffEvent;
                        if (off_event.note_number == on_event.note_number)
                            return true;
                    }
                }
                if(@event is NoteOnEvent)
                    on_event = on_event ?? @event as NoteOnEvent;
            }
            return false;
        }

        public static IEnumerable<int> GetIndicesAfter(ITimedEventInfo[] event_infos, float time_beats)
        {
            // Index immediately after time
            int start = FirstIndexAfterTime(event_infos, time_beats);
            for (int i = start; i < event_infos.Length; i++)
            {
                yield return i;
            }
        }

        /// <summary>
        /// Get all the notes after a given time
        /// </summary>
        /// <param name="time_beats">Time in beats notes, NOT seconds!</param>
        /// <returns></returns>
        public static IEnumerable<ITimedEventInfo> GetEventsAfter(
            ITimedEventInfo[] event_infos,
            float time_beats)
        {
            foreach (int i in GetIndicesAfter(event_infos, time_beats))
                yield return event_infos[i];
        }

        public static IEnumerable<ITimedEventInfo> GetEventsAfter(
            ITimedEventInfo[] event_infos,
            int index)
        {
            for (; index < event_infos.Length; index++)
                yield return event_infos[index];
        }

        public static int FirstIndexAfterTime(ITimedEventInfo[] event_infos, float beats)
        {
            return FirstIndexAfterTime(event_infos, beats, event_infos.Length / 2);
        }

        private static int FirstIndexAfterTime(ITimedEventInfo[] event_infos, float beats, int current_index)
        {
            return _FirstIndexAfterTime(event_infos, beats, current_index, 2);
        }

        private static int _FirstIndexAfterTime(
            ITimedEventInfo[] event_infos,
            float time_beats,
            int current_index,
            int depth)
        {
            // Edge cases
            if (current_index <= 0)
                return 0;
            int last = event_infos.Length - 1;
            if (current_index >= last)
                return last;

            // Essentially a modified binary search
            var previous = event_infos[current_index - 1];
            var here = event_infos[current_index];
            if (previous.TimeBeats < time_beats && here.TimeBeats >= time_beats)
                return current_index;

            int slide = event_infos.Length >> depth;
            if (here.TimeBeats > time_beats)
                slide = -slide;
            return _FirstIndexAfterTime(event_infos, time_beats, current_index + slide, depth + 1);
        }

        public static int GetEventsUntil(
            ITimedEventInfo[] event_infos,
            float time_beats_until,
            int index,
            out IEnumerable<ITimedEventInfo> events_until)
        {
            var events_until_list = new List<ITimedEventInfo>();
            for (; event_infos[index].TimeBeats < time_beats_until; index++)
                events_until_list.Add(event_infos[index]);
            events_until = events_until_list;
            return index;
        }

    }
}