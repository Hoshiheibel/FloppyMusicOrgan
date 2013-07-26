namespace MidiParser.Entities.MidiEvents
{
    public class NoteOnEvent : BaseMidiChannelEvent
    {
        public int Note { get; set; }
        public int Velocity { get; set; }
    }
}
