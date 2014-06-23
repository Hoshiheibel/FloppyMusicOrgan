using System;
using System.Collections.Generic;
using MidiParser.Entities.MidiEvents;

namespace MidiPlayer.Events
{
    public class ComDataSentEventArgs : EventArgs
    {
        public List<BaseMidiChannelEvent> Messages { get; set; }
    }
}
