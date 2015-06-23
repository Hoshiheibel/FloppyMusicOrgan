namespace MidiParser.Entities.Enums
{
    public enum MidiEventType
    {
        TempoChange = 0x51,
        AllNotesOff = 0x7B,
        NoteOff = 0x80,
        NoteOn = 0x90,
        KeyAftertouch = 0xA0,
        ControllerChange = 0xB0,
        ProgramChange = 0xC0,
        ChannelAftertouch = 0xD0,
        PitchBend = 0xE0,
        MetaEvent = 0xFF,
        SysExContinuationEvent = 0xF7,
        SysExEvent = 0xF0
    }
}
