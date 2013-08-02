using System.Collections.Generic;
using MidiParser.Entities.Enums;

namespace MidiParser.Entities
{
    public class ParsedMidiFile
    {
        public MidiHeader MidiHeader { get; private set; }
        public List<MidiTrack> Tracks { get; private set; }
        public List<ConvertedMidiTrack> ConvertedTracks { get; set; } 
        public TimeDivisionType TimeDivisionType { get; internal set; }

        internal ParsedMidiFile()
        {
            MidiHeader = new MidiHeader();
            Tracks = new List<MidiTrack>();
            ConvertedTracks = new List<ConvertedMidiTrack>();
        }
    }
}
