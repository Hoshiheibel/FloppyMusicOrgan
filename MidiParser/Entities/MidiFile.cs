using System.Collections.Generic;

namespace MidiParser.Entities
{
    class MidiFile
    {
        public MidiHeader MidiHeader { get; set; }
        public List<MidiTrack> Tracks { get; set; }
        public int NumberOfTracks { get; set; }

        public MidiFile()
        {
            MidiHeader = new MidiHeader();
            Tracks = new List<MidiTrack>();
        }
    }
}
