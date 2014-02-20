using System;
using System.Linq;
using ComOutput;
using MidiToArduinoConverter;

namespace MidiPlayer
{
    public class Player
    {
        public EventHandler<TimePositionChangedEventArgs> TimePositionChanged; 
        public EventHandler<EventArgs> PlaybackFinished; 
        private readonly ComStreamer _comStreamerMusic;
        private readonly MicroTimer _timer;
        private ConvertedMidiTrack _track;
        private int _currentTrackPosition;
        private bool _isStopped;

        public Player(ComStreamer comStreamer)
        {
            _comStreamerMusic = comStreamer;
            _timer = new MicroTimer();
            _timer.MicroTimerElapsed += MicroTimerOnElapsed;
        }

        ~Player()
        {
            _timer.MicroTimerElapsed -= MicroTimerOnElapsed;
            _comStreamerMusic.Dispose();
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
            _currentTrackPosition = 0;

            TimePositionChanged.Invoke(this, new TimePositionChangedEventArgs
            {
                NewDeltaTimePosition = 0,
                NewTimePosition = new TimeSpan(0)
            });

            if (_comStreamerMusic.IsConnected)
                _comStreamerMusic.SendStopCommand();
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

                if (message.ComMessage != null)
                {
                    _comStreamerMusic.SendCommand(message.ComMessage);
                }

                _currentTrackPosition++;

                TimePositionChanged.Invoke(this, new TimePositionChangedEventArgs
                {
                    NewDeltaTimePosition = message.AbsoluteDeltaTimePosition,
                    NewTimePosition = message.AbsoluteTimePosition
                });
            }
            else
            {
                StopPlayback();

                if (PlaybackFinished != null)
                    PlaybackFinished.Invoke(this, new EventArgs());
            }
        }

        public void GoToSelectedSongPosition(double deltaTimePosition)
        {
            var minDistance = _track.MessageList.Min(n => Math.Abs(deltaTimePosition - n.AbsoluteDeltaTimePosition));
            var closest = _track.MessageList.First(n => Math.Abs(deltaTimePosition - n.AbsoluteDeltaTimePosition) == minDistance);

            _comStreamerMusic.SendStopCommand();
            _currentTrackPosition = _track.MessageList.IndexOf(closest);
        }

        public void PausePlayback()
        {
            _timer.Stop();
            _isStopped = true;

            if (_comStreamerMusic.IsConnected)
                _comStreamerMusic.SendStopCommand();
        }
    }
}
