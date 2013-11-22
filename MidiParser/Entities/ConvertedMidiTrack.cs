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

    public class MessageListComparer : IComparer<ArduinoMessage>
    {
        public int Compare(ArduinoMessage x, ArduinoMessage y)
        {
            if (x.AbsoluteDeltaTimePosition < y.AbsoluteDeltaTimePosition)
                return -1;
            else if (x.AbsoluteDeltaTimePosition > y.AbsoluteDeltaTimePosition)
                return 1;

            return 0;
        }
    }
}
