using System.Collections.Generic;
using MidiParser.Entities.MidiEvents;

namespace MidiParser.Entities
{
    public class ConvertedMidiTrack
    {
        private readonly List<ArduinoMessage> _messageList;
        private const int ArduinoResolution = 40;
        private static int[] _microPeriods =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            30578, 28861, 27242, 25713, 24270, 22909, 21622, 20409, 19263, 18182, 17161, 16198, //C1 - B1
            15289, 14436, 13621, 12856, 12135, 11454, 10811, 10205, 9632, 9091, 8581, 8099, //C2 - B2
            7645, 7218, 6811, 6428, 6068, 5727, 5406, 5103, 4816, 4546, 4291, 4050, //C3 - B3
            3823, 3609, 3406, 3214, 3034, 2864, 2703, 2552, 2408, 2273, 2146, 2025, //C4 - B4
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        public ConvertedMidiTrack()
        {
            _messageList = new List<ArduinoMessage>();
        }

        public void BuildTimeLine(MidiTrack track)
        {
            foreach (var midiEvent in track.TrackEventChain)
            {
                ParseAndAddMessage(midiEvent);
            }
        }

        private void ParseAndAddMessage(BaseMidiChannelEvent midiEvent)
        {
            if (midiEvent is NoteOnEvent)
            {
                _messageList.Add(new ArduinoMessage
                {
                    DeltaTime = midiEvent.DeltaTime,
                    ComMessage = new[]
                    {
                        (byte) (midiEvent.ChannelNumber + 1),
                        (byte) (_microPeriods[((NoteOnEvent)midiEvent).Note]/ArduinoResolution*2)
                    }
                });
            }
            else if (midiEvent is NoteOffEvent)
            {
                _messageList.Add(new ArduinoMessage
                {
                    DeltaTime = midiEvent.DeltaTime,
                    ComMessage = new[]
                    {
                        (byte) (midiEvent.ChannelNumber + 1),
                        (byte) (0)
                    }
                });
            }
        }
    }
}
