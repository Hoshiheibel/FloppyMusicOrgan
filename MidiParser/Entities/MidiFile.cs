using System.Collections.Generic;

namespace MidiParser.Entities
{
    class MidiFile
    {
        public MidiHeader MidiHeader { get; private set; }
        public List<MidiTrack> Tracks { get; internal set; }

        internal MidiFile()
        {
            MidiHeader = new MidiHeader();
            Tracks = new List<MidiTrack>();
        }
    }
}
