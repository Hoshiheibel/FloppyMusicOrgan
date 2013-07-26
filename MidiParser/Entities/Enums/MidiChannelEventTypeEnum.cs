namespace MidiParser.Entities.Enums
{
    public enum MidiChannelEventTypeEnum
    {
        NoteOf = 0x8,
        NoteOn = 0x9,
        NoteAftertouch = 0xA,
        Controller = 0xB,
        ProgramChange = 0xC,
        ChannelAftertouch = 0xD,
        PitchBend = 0xE
    }
}
