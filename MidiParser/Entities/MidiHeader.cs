namespace MidiParser.Entities
{
    public class MidiHeader
    {
        public string FileIdentifier { get; internal set; }
        public int HeaderLength { get; internal set; }
        public MidiFormatTypeEnum Format { get; internal set; }
    }
}
