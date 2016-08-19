using System;
using UnityEngine;

namespace RoaringFangs.Formats.MIDI
{
    public interface ITimedEventInfo
    {
        float TimeBeats { get; set; }
        float TimeMeasures { get; set; }
    }
}