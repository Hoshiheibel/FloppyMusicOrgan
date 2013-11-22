using System;
using System.Threading;
using ComOutput;
using MidiParser.Entities;
using MidiParser.Entities.MidiFile;

namespace MidiPlayer
{
    public class Player
    {
        private readonly ComStreamer _comStreamer;
        private readonly MicroTimer _timer;
        private ConvertedMidiTrack _track;
        private int _currentTrackPosition;
        private double _timeModificator;
        private int _timeDivision;

        public Player(ComStreamer comStreamer)
        {
            _comStreamer = comStreamer;
            _timer = new MicroTimer();
        }

        public void Play(MidiFile midiFile)
        {
            _timeDivision = midiFile.FileHeader.TimeDivision;
            RecalculateBPM(midiFile.BPM);

            _timer.MicroTimerElapsed += MicroTimerOnElapsed;
            _track = midiFile.ConvertedTrack;
            DateTime.Now.Add(new TimeSpan(0, 0, 0, 1));
            _timer.Interval = 1000;
            _timer.Enabled = true;
            _timer.Start();
        }

        private void MicroTimerOnElapsed(object sender, MicroTimerEventArgs elapsedEventArgs)
        {
            if (_currentTrackPosition < _track.MessageList.Count)
            {
                var message = _track.MessageList[_currentTrackPosition];

                if (_track.MessageList.Count > _currentTrackPosition + 1)
                {
                    var nextMessage = _track.MessageList[_currentTrackPosition + 1];
                    _timer.Interval = (long) ((double) nextMessage.RelativeTimePosition * _timeModificator * 1000 * 1000);
                }
                else
                {
                    _timer.Interval = 100;
                }

                if (message is TempoChangeDummyMessage)
                {
                    RecalculateBPM(((TempoChangeDummyMessage)message).BPM);
                    _currentTrackPosition++;
                }
                
                if (message.ComMessage != null)
                {
                    _comStreamer.SendCommand(message.ComMessage);
                    _currentTrackPosition++;
                }
            }
            else
            {
                _timer.Stop();
            }
        }

        private void RecalculateBPM(int bpm)
        {
            double secondsPerBeat = ((double)60D / (double)bpm);
            _timeModificator = secondsPerBeat / (double)_timeDivision;
        }

        public void StopPlayback()
        {
            _timer.Stop();
        }
    }
}
