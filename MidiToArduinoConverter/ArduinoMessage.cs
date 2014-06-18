using System;
using System.Collections.Generic;
using MidiParser.Entities.MidiEvents;

namespace MidiToArduinoConverter
{
    public class ArduinoMessage
    {
        public TimeSpan AbsoluteTimePosition { get; set; }
        public long TimerIntervallToNextEvent { get; set; }
        public byte[] ComMessage { get; set; }
        public long AbsoluteDeltaTimePosition { get; set; }
        public long RelativeTimePosition { get; set; }
        public List<BaseMidiChannelEvent> OriginalMidiEvents { get; set; }
    }
}
