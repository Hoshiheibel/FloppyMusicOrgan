using System.Collections.Generic;
using MidiParser.Entities.Enums;

namespace MidiParser.Entities.MidiFile
{
    public class MidiFile
    {
        public FileHeader FileHeader { get; private set; }
        public List<Track> Tracks { get; private set; }
        public TimeDivisionTypeEnum TimeDivisionType { get; internal set; }
        public int BPM { get; set; }

        internal MidiFile()
        {
            FileHeader = new FileHeader();
            Tracks = new List<Track>();
            BPM = 120;
        }
    }
}
