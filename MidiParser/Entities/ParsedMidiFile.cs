using System.Collections.Generic;

namespace MidiParser.Entities
{
    public class ParsedMidiFile
    {
        public MidiHeader MidiHeader { get; private set; }
        public List<MidiTrack> Tracks { get; internal set; }

        internal ParsedMidiFile()
        {
            MidiHeader = new MidiHeader();
            Tracks = new List<MidiTrack>();
        }
    }
}
