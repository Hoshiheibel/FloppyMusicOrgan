using System;

namespace MidiPlayer
{
    public class TimePositionChangedEventArgs : EventArgs
    {
        public long NewDeltaTimePosition { get; set; }
        public TimeSpan NewTimePosition { get; set; }
    }
}
