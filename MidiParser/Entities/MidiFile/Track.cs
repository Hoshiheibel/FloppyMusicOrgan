using System.Collections.Generic;
using MidiParser.Entities.MidiEvents;

namespace MidiParser.Entities.MidiFile
{
    public class Track
    {
        public TrackHeader TrackHeader { get; set; }
        public int TrackLength { get; set; }
        public List<BaseMidiChannelEvent> TrackEventChain { get; set; }

        public Track()
        {
            TrackHeader = new TrackHeader();
            TrackEventChain = new List<BaseMidiChannelEvent>();
        }
    }
}
