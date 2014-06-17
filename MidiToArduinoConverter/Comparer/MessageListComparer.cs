using System.Collections.Generic;

namespace MidiToArduinoConverter.Comparer
{
    public class MessageListComparer : IComparer<ArduinoMessage>
    {
        public int Compare(ArduinoMessage x, ArduinoMessage y)
        {
            if (x.AbsoluteDeltaTimePosition < y.AbsoluteDeltaTimePosition)
                return -1;
            if (x.AbsoluteDeltaTimePosition > y.AbsoluteDeltaTimePosition)
                return 1;

            return 0;
        }
    }
}