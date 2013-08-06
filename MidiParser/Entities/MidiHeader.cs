using MidiParser.Entities.Enums;

namespace MidiParser.Entities
{
    public class MidiHeader
    {
        public string FileIdentifier { get; internal set; }
        public int HeaderLength { get; internal set; }
        public MidiFormatTypeEnum Format { get; internal set; }
        public int NumberOfTracks { get; internal set; }
        public int TimeDivision { get; internal set; }
    }
}
