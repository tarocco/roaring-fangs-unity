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
    public class MIDIEventFilter : MIDIHelperBase
    {
        private List<Type> _Include;
        private List<Type> _Exclude;
            
        /// <summary>
        /// Preset filter for Note on/off events
        /// </summary>
        public static MIDIEventFilter NotesOnOffFilter
        {
            get { return _NotesOnOffFilter; }
        }
        private static readonly MIDIEventFilter _NotesOnOffFilter = new MIDIEventFilter();

        /// <summary>
        /// Preset filter for track name events
        /// </summary>
        public static MIDIEventFilter TrackNameFilter
        {
            get { return _TrackNameFilter; }
        }
        private static readonly MIDIEventFilter _TrackNameFilter = new MIDIEventFilter();
        static MIDIEventFilter()
        {
            NotesOnOffFilter.pInclude(
                typeof(Midi.Events.ChannelEvents.NoteOnEvent),
                typeof(Midi.Events.ChannelEvents.NoteOffEvent));
            TrackNameFilter.pInclude(
                typeof(Midi.Events.MetaEvents.SequenceOrTrackNameEvent));
        }
        public MIDIEventFilter()
        {
            _Include = new List<Type>();
            _Exclude = new List<Type>();
        }
        protected override void pInclude(params Type[] types)
        {
            _Include.AddRange(types);
        }
        protected override void pExclude(params Type[] types)
        {
            _Include.AddRange(types);
        }
        public bool Accepts(MidiEvent m_e)
        {
            if (_Include.Count == 0)
            {
                if (_Exclude.Count == 0)
                    return true;
                else
                    return
                        !_Exclude.Contains(m_e.GetType());
            }
            else
            {
                if (_Exclude.Count == 0)
                    return
                        _Include.Contains(m_e.GetType());
                else
                    return
                        _Include.Contains(m_e.GetType()) &&
                        !_Exclude.Contains(m_e.GetType());
            }
        }
    }
}