using System;
using ComOutput;
using MidiToArduinoConverter;

namespace MidiPlayer
{
    public class Player
    {
        public EventHandler<TimePositionChangedEventArgs> TimePositionChanged; 
        private readonly ComStreamer _comStreamer;
        private readonly MicroTimer _timer;
        private ConvertedMidiTrack _track;
        private int _currentTrackPosition;
        private bool _isStopped;

        public Player(ComStreamer comStreamer)
        {
            _comStreamer = comStreamer;
            _timer = new MicroTimer();
            _timer.MicroTimerElapsed += MicroTimerOnElapsed;
        }

        ~Player()
        {
            _timer.MicroTimerElapsed -= MicroTimerOnElapsed;
            _comStreamer.Dispose();
        }

        public void Play(ConvertedMidiTrack convertedMidiFile)
        {
            _currentTrackPosition = 0;
            _isStopped = false;

            _track = convertedMidiFile;
            _timer.Interval = 1000;
            _timer.Enabled = true;
            _timer.Start();
        }

        public void StopPlayback()
        {
            _timer.Stop();
            _isStopped = true;

            if (_comStreamer.IsConnected)
                _comStreamer.SendStopCommand();
        }

        public void ResumePlayback()
        {
            _timer.Start();
            _isStopped = false;
        }

        private void MicroTimerOnElapsed(object sender, MicroTimerEventArgs elapsedEventArgs)
        {
            if (_isStopped)
                return;

            if (_currentTrackPosition < _track.MessageList.Count)
            {
                var message = _track.MessageList[_currentTrackPosition];
                _timer.Interval = message.TimerIntervallToNextEvent;

                _comStreamer.SendCommand(message.ComMessage);
                _currentTrackPosition++;

                TimePositionChanged.Invoke(this, new TimePositionChangedEventArgs
                {
                    NewDeltaTimePosition = message.AbsoluteDeltaTimePosition,
                    NewTimePosition = message.AbsoluteTimePosition
                });
            }
            else
                StopPlayback();
        }
    }
}
