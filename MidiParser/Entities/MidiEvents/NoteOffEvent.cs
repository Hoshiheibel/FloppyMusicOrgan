namespace MidiParser.Entities.MidiEvents
{
    public class NoteOffEvent : BaseMidiChannelEvent
    {
        public int Note { get; set; }
        public int Velocity { get; set; }
    }
}
