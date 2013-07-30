using System.Collections.Generic;
using MidiParser.Entities.MidiEvents;

namespace MidiParser.Entities
{
    public class MidiTrack
    {
        public TrackHeader TrackHeader { get; set; }
        public int TrackLength { get; set; }
        public List<BaseMidiChannelEvent> TrackEventChain { get; set; }

        public MidiTrack()
        {
            TrackHeader = new TrackHeader();
            TrackEventChain = new List<BaseMidiChannelEvent>();
        }
    }
}
