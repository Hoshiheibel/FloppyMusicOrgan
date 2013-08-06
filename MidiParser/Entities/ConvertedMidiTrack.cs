using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiParser.Entities
{
    public class ConvertedMidiTrack
    {
        public List<ArduinoMessage> MessageList { get; set; } 

        public ConvertedMidiTrack()
        {
            MessageList = new List<ArduinoMessage>();
        }
    }

    public class MessageListComparer : IComparer<ArduinoMessage>
    {
        public int Compare(ArduinoMessage x, ArduinoMessage y)
        {
            if (x.AbsoluteTimePosition < y.AbsoluteTimePosition)
                return -1;
            else if (x.AbsoluteTimePosition > y.AbsoluteTimePosition)
                return 1;

            return 0;
        }
    }
}
