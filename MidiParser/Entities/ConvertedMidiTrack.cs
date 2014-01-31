using System.Collections.Generic;

namespace MidiParser.Entities
{
    public class ConvertedMidiTrack
    {
        public List<ArduinoMessage> MessageList { get; set; }
        public int BPM { get; set; }

        public ConvertedMidiTrack()
        {
            MessageList = new List<ArduinoMessage>();
        }
    }
}
