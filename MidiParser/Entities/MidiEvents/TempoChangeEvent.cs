namespace MidiParser.Entities.MidiEvents
{
    public class TempoChangeEvent : BaseMidiChannelEvent
    {
        public int BPM { get; set; }
    }
}
