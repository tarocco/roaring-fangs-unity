namespace RoaringFangs.Formats.MIDI
{
    public interface INoteEventInfo : ITimedEventInfo
    {
        float DurationBeats { get; set; }
        int NoteNumber { get; set; }
    }
}