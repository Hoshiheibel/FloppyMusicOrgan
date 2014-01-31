using System;

namespace MidiToArduinoConverter
{
    public class ArduinoMessage
    {
        public TimeSpan AbsoluteTimePosition { get; set; }
        public byte[] ComMessage { get; set; }
        public long AbsoluteDeltaTimePosition { get; set; }
        public long RelativeTimePosition { get; set; }
    }
}
