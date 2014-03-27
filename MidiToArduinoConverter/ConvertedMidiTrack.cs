using System.Collections.Generic;
using MidiParser.Entities.MidiFile;

namespace MidiToArduinoConverter
{
    public class ConvertedMidiTrack
    {
        public MidiFile MidiFile { get; set; }
        public List<ArduinoMessage> MessageList { get; set; }
        public int BPM { get; set; }

        public ConvertedMidiTrack()
        {
            MessageList = new List<ArduinoMessage>();
        }
    }
}
