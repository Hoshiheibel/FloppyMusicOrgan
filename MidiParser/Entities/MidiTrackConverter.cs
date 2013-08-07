using System;
using System.Collections.Generic;
using System.Linq;
using MidiParser.Entities.MidiEvents;

namespace MidiParser.Entities
{
    public class MidiTrackConverter
    {
        private const int ArduinoResolution = 40;
        private static readonly int[] MicroPeriods =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            30578, 28861, 27242, 25713, 24270, 22909, 21622, 20409, 19263, 18182, 17161, 16198, //C1 - B1
            15289, 14436, 13621, 12856, 12135, 11454, 10811, 10205, 9632, 9091, 8581, 8099, //C2 - B2
            7645, 7218, 6811, 6428, 6068, 5727, 5406, 5103, 4816, 4546, 4291, 4050, //C3 - B3
            3823, 3609, 3406, 3214, 3034, 2864, 2703, 2552, 2408, 2273, 2146, 2025, //C4 - B4
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        private long _ticksPerSecond;

        public ConvertedMidiTrack BuildTimeLine(IEnumerable<MidiTrack> tracks, long ticksPerSecond)
        {
            var convertedMidiTrack = new ConvertedMidiTrack();
            _ticksPerSecond = ticksPerSecond;

            foreach (var midiTrack in tracks)
            {
                var currentTimePosition = new TimeSpan();

                foreach (var midiEvent in midiTrack.TrackEventChain)
                {
                    currentTimePosition = ParseAndAddMessage(midiEvent, convertedMidiTrack, currentTimePosition);
                }
            }

            convertedMidiTrack.MessageList.Sort(new MessageListComparer());
            return convertedMidiTrack;
        }

        private TimeSpan ParseAndAddMessage(BaseMidiChannelEvent midiEvent, ConvertedMidiTrack convertedMidiTrack, TimeSpan lastTime)
        {
            var timePosition = lastTime.Add(new TimeSpan(0, 0, 0, 0, (int)(midiEvent.DeltaTime * _ticksPerSecond / 100)));

            if (midiEvent is NoteOnEvent)
            {
                var message = convertedMidiTrack.MessageList.SingleOrDefault(m => m.AbsoluteTimePosition.Equals(timePosition));
                var period = MicroPeriods[((NoteOnEvent) midiEvent).Note] / (ArduinoResolution * 2);

                if (message != null)
                {
                    message.ComMessage = message.ComMessage.Concat(new[]
                    {
                        (byte) ((midiEvent.ChannelNumber + 1) * 2),
                        (byte) ((period >> 8) & 0xFF),
                        (byte) (period & 0xFF)
                    })
                    .ToArray();
                }
                else
                {
                    convertedMidiTrack.MessageList.Add(new ArduinoMessage
                    {
                        AbsoluteTimePosition = timePosition,
                        RelativeTimePosition = midiEvent.DeltaTime * _ticksPerSecond,
                        ComMessage = new[]
                        {
                            (byte) ((midiEvent.ChannelNumber + 1) * 2),
                            (byte) ((period >> 8) & 0xFF),
                            (byte) (period & 0xFF)
                        }
                    });
                }
            }
            else if (midiEvent is NoteOffEvent)
            {
                var message = convertedMidiTrack.MessageList.SingleOrDefault(m => m.AbsoluteTimePosition.Equals(timePosition));

                if (message != null)
                {
                    message.ComMessage = message.ComMessage.Concat(new[]
                    {
                        (byte) ((midiEvent.ChannelNumber + 1) * 2),
                        (byte) 0,
                        (byte) 0
                    })
                    .ToArray();
                }
                else
                {
                    convertedMidiTrack.MessageList.Add(new ArduinoMessage
                    {
                        AbsoluteTimePosition = timePosition,
                        RelativeTimePosition = midiEvent.DeltaTime * _ticksPerSecond,
                        ComMessage = new[]
                        {
                            (byte) ((midiEvent.ChannelNumber + 1) * 2),
                            (byte) 0,
                            (byte) 0
                        }
                    });
                }
            }

            return timePosition;
        }
    }
}
