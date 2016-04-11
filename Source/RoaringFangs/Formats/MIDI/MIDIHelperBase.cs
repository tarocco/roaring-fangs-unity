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
    public abstract class MIDIHelperBase
    {
        // Faux friend pattern.
        #region Virtual Methods
        protected virtual List<MIDIEventHelper> pEvents
        {
            get { return null; }
            set {  }
        }
        protected virtual string pName
        {
            get { return null; }
            set { }
        }
        protected virtual void pInclude(params Type[]  type)
        {
        }
        protected virtual void pExclude(params Type[] type)
        {
        }
        #endregion
        #region Static Methods
        protected static List<MIDIEventHelper> GetInternalListOfEvents(MIDIHelperBase self)
        {
            return self.pEvents;
        }
        protected static string GetName(MIDIHelperBase self)
        {
            return self.pName;
        }
        protected static void SetName(MIDIHelperBase self, string value)
        {
            self.pName = value;
        }
        protected static void Include(MIDIHelperBase self, Type type)
        {
            self.pInclude(type);
        }
        protected static void Exclude(MIDIHelperBase self, Type type)
        {
            self.pExclude(type);
        }
        #endregion
    }
}