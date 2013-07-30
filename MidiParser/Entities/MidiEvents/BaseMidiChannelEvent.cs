namespace MidiParser.Entities.MidiEvents
{
    public class BaseMidiChannelEvent
    {
        public long DeltaTime { get; set; }
        public int ChannelNumber { get; set; }
    }
}
