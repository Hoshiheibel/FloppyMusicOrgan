using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiParser.Entities
{
    public class MidiTrack
    {
        public TrackHeader TrackHeader { get; set; }
        public string TrackDescription { get; set; }
        public int TrackLength { get; set; }

        public MidiTrack()
        {
            TrackHeader = new TrackHeader();
        }
    }
}
