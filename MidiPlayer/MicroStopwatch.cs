using System;

//
// High performance timing, taken from "http://www.codeproject.com/Articles/98346/Microsecond-and-Millisecond-NET-Timer"
//
namespace MidiPlayer
{
    public class MicroStopwatch : System.Diagnostics.Stopwatch
    {
        readonly double _microSecPerTick =
            1000000D / Frequency;

        public MicroStopwatch()
        {
            if (!IsHighResolution)
            {
                throw new Exception("On this system the high-resolution " +
                                    "performance counter is not available");
            }
        }

        public long ElapsedMicroseconds
        {
            get
            {
                return (long)(ElapsedTicks * _microSecPerTick);
            }
        }
    }
}