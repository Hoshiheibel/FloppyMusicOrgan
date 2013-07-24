namespace MidiParser.Entities
{
    public class MidiHeader
    {
        public string FileIdentifier { get; set; }
        public int HeaderLength { get; set; }
        public MidiFormatTypeEnum MidiFormatType { get; set; }
    }
}
