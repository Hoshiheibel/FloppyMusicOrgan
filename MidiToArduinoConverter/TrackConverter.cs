using System;
using System.Collections.Generic;
using System.Linq;
using MidiParser.Entities.Enums;
using MidiParser.Entities.MidiEvents;
using MidiParser.Entities.MidiFile;
using MidiToArduinoConverter.Comparer;

namespace MidiToArduinoConverter
{
    public class TrackConverter
    {
        private const int ArduinoResolution = 40;
        private long _ticksPerSecond;
        private long _currentAbsoluteDeltaPosition;
        private double _currentTimeModificator;
        private int _currentTimeDivision;
        private readonly int[] _periods;

        // 440Hz Tuning
        private static readonly int[] MicroPeriods440Hz =
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

        // 432Hz Tuning
        private static readonly int[] MicroPeriods432Hz =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            31143, 29394, 27747, 26192, 24722, 23332, 22022, 20786, 19619, 18519, 17479, 16498, //C1 - B1
            15571, 14697, 13873, 13094, 12359, 11666, 11011, 10393, 9810, 9259, 8740, 8249, //C2 - B2
            7786, 7349, 6937, 6547, 6180, 5833, 5506, 5197, 4905, 4630, 4370, 4125, //C3 - B3
            3893, 3675, 3468, 3274, 3090, 2916, 2753, 2598, 2452, 2315, 2185, 2062, //C4 - B4
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        public TrackConverter(TuningFrequencyEnum tuningFrequency = TuningFrequencyEnum.Frequency440Hz)
        {
            switch (tuningFrequency)
            {
                case TuningFrequencyEnum.Frequency432Hz:
                    _periods = MicroPeriods432Hz;
                    break;

                case TuningFrequencyEnum.Frequency440Hz:
                    _periods = MicroPeriods440Hz;
                    break;

                default:
                    _periods = MicroPeriods440Hz;
                    break;
            }
        }

        public ConvertedMidiTrack Convert(MidiFile midiFile)
        {
            _currentTimeDivision = midiFile.FileHeader.TimeDivision;
            RecalculateBPM(midiFile.BPM);

            var convertedTrack = BuildTimeLine(midiFile.Tracks, midiFile.FileHeader.TimeDivision, midiFile.BPM);
            convertedTrack.MidiFile = midiFile;
            return convertedTrack;
        }

        private ConvertedMidiTrack BuildTimeLine(IEnumerable<Track> tracks, long ticksPerSecond, int bpm)
        {
            var convertedMidiTrack = new ConvertedMidiTrack
            {
                BPM = bpm
            };

            _ticksPerSecond = ticksPerSecond;

            foreach (var midiTrack in tracks)
            {
                var currentTimePosition = new TimeSpan();
                _currentAbsoluteDeltaPosition = 0;

                foreach (var midiEvent in midiTrack.TrackEventChain)
                {
                    currentTimePosition = ParseAndAddMessage(midiEvent, convertedMidiTrack, currentTimePosition);
                }
            }

            convertedMidiTrack.MessageList.Sort(new MessageListComparer());
            RecalculateTimePositions(convertedMidiTrack);
            return convertedMidiTrack;
        }

        private void RecalculateTimePositions(ConvertedMidiTrack track)
        {
            ArduinoMessage lastMesssage = null;

            foreach (var message in track.MessageList)
            {
                if (lastMesssage != null)
                {
                    message.RelativeTimePosition = message.AbsoluteDeltaTimePosition -
                                                   lastMesssage.AbsoluteDeltaTimePosition;

                    lastMesssage.TimerIntervallToNextEvent = CalculateNextTimerTick(message.RelativeTimePosition);
                    
                    message.AbsoluteTimePosition =
                        lastMesssage.AbsoluteTimePosition.Add(new TimeSpan(0, 0, 0, 0, ((int)lastMesssage.TimerIntervallToNextEvent / 1000)));
                }

                var tempoMessage = message as TempoChangeDummyMessage;

                if (tempoMessage != null)
                    RecalculateBPM(tempoMessage.BPM);

                lastMesssage = message;
            }
        }

        private TimeSpan ParseAndAddMessage(BaseMidiChannelEvent midiEvent, ConvertedMidiTrack convertedMidiTrack, TimeSpan lastTime)
        {
            var timePosition = lastTime.Add(new TimeSpan(0, 0, 0, 0, (int)(midiEvent.DeltaTime * _ticksPerSecond / 100)));
            _currentAbsoluteDeltaPosition += midiEvent.DeltaTime;

            if (midiEvent is NoteOnEvent)
            {
                var message = convertedMidiTrack.MessageList.SingleOrDefault(m => m.AbsoluteDeltaTimePosition.Equals(_currentAbsoluteDeltaPosition));
                var period = _periods[((NoteOnEvent) midiEvent).Note] / (ArduinoResolution * 2);

                if (message != null)
                {
                    if (message.ComMessage == null)
                        message.ComMessage = new byte[0];

                    message.ComMessage = message.ComMessage.Concat(new[]
                    {
                        (byte) (midiEvent.ChannelNumber + 1),
                        (byte) ((period >> 8) & 0xFF),
                        (byte) (period & 0xFF)
                    })
                    .ToArray();

                    message.OriginalMidiEvents.Add(midiEvent);
                }
                else
                {
                    convertedMidiTrack.MessageList.Add(new ArduinoMessage
                    {
                        AbsoluteTimePosition = timePosition,
                        AbsoluteDeltaTimePosition = _currentAbsoluteDeltaPosition,
                        RelativeTimePosition = midiEvent.DeltaTime*_ticksPerSecond,
                        ComMessage = new[]
                        {
                            (byte) (midiEvent.ChannelNumber + 1),
                            (byte) ((period >> 8) & 0xFF),
                            (byte) (period & 0xFF)
                        },
                        OriginalMidiEvents = new List<BaseMidiChannelEvent>
                        {
                            midiEvent
                        }
                    });
                }
            }
            else if (midiEvent is NoteOffEvent)
            {
                var message = convertedMidiTrack.MessageList.SingleOrDefault(m => m.AbsoluteDeltaTimePosition.Equals(_currentAbsoluteDeltaPosition));

                if (message != null)
                {
                    if (message.ComMessage == null)
                        message.ComMessage = new byte[0];

                    message.ComMessage = message.ComMessage.Concat(new[]
                    {
                        (byte) (midiEvent.ChannelNumber + 1),
                        (byte) 0,
                        (byte) 0
                    })
                    .ToArray();

                    message.OriginalMidiEvents.Add(midiEvent);
                }
                else
                {
                    convertedMidiTrack.MessageList.Add(new ArduinoMessage
                    {
                        AbsoluteTimePosition = timePosition,
                        AbsoluteDeltaTimePosition = _currentAbsoluteDeltaPosition,
                        RelativeTimePosition = midiEvent.DeltaTime * _ticksPerSecond,
                        ComMessage = new[]
                        {
                            (byte) (midiEvent.ChannelNumber + 1),
                            (byte) 0,
                            (byte) 0
                        },
                        OriginalMidiEvents = new List<BaseMidiChannelEvent>
                        {
                            midiEvent
                        }
                    });
                }
            }
            else if (midiEvent is TempoChangeEvent)
            {
                var message =
                    convertedMidiTrack.MessageList.SingleOrDefault(
                        m => m.AbsoluteDeltaTimePosition.Equals(_currentAbsoluteDeltaPosition)
                             && m is TempoChangeDummyMessage);

                var message2 =
                    convertedMidiTrack.MessageList.SingleOrDefault(
                        m => m.AbsoluteDeltaTimePosition.Equals(_currentAbsoluteDeltaPosition));

                if (message != null)
                {
                    ((TempoChangeDummyMessage) message).BPM = ((TempoChangeEvent)midiEvent).BPM;
                }
                else if (message2 != null)
                {
                    var newMessage = new TempoChangeDummyMessage
                    {
                        AbsoluteTimePosition = timePosition,
                        AbsoluteDeltaTimePosition = _currentAbsoluteDeltaPosition,
                        RelativeTimePosition = midiEvent.DeltaTime * _ticksPerSecond,
                        BPM = ((TempoChangeEvent)midiEvent).BPM,
                        ComMessage = message2.ComMessage
                    };

                    convertedMidiTrack.MessageList.Remove(message2);
                    convertedMidiTrack.MessageList.Add(newMessage);
                }
                else
                {
                    convertedMidiTrack.MessageList.Add(new TempoChangeDummyMessage
                    {
                        AbsoluteTimePosition = timePosition,
                        AbsoluteDeltaTimePosition = _currentAbsoluteDeltaPosition,
                        RelativeTimePosition = midiEvent.DeltaTime * _ticksPerSecond,
                        BPM = ((TempoChangeEvent)midiEvent).BPM,
                        OriginalMidiEvents = new List<BaseMidiChannelEvent>
                        {
                            midiEvent
                        }
                    });
                }
            }

            return timePosition;
        }

        private void RecalculateBPM(int bpm)
        {
            double secondsPerBeat = (60D / bpm);
            _currentTimeModificator = secondsPerBeat / _currentTimeDivision;
        }

        private long CalculateNextTimerTick(long relativeTimePosition)
        {
            return (long)(relativeTimePosition * _currentTimeModificator * 1000 * 1000);
        }
    }
}
