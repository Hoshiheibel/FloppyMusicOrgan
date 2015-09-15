using System.Collections.Generic;
using FloppyMusicOrgan.Common;
using MidiParser.Entities.MidiEvents;
using MidiPlayer;
using MidiPlayer.Events;

namespace FloppyMusicOrgan.ViewModels
{
    class EqualizerViewModel : PropertyChangedBase
    {
        private MicroTimer _timer;
        public EqualizerViewModel()
        {
            Bars = new List<EqualizerBarViewModel>();

            for (int i = 0; i < 16; i++)
            {
                Bars.Add(new EqualizerBarViewModel());
            }

            _timer = new MicroTimer(1000 * 15);
            _timer.MicroTimerElapsed += TimerOnMicroTimerElapsed;
            _timer.Start();
        }

        private void TimerOnMicroTimerElapsed(object sender, MicroTimerEventArgs timerEventArgs)
        {
            foreach (var bar in Bars)
            {
                if (bar.IsDecaying)
                {
                    bar.Value--;

                    if (bar.Value == 0)
                        bar.IsDecaying = false;
                }
            }
        }

        public List<EqualizerBarViewModel> Bars { get; private set; }

        public void SetCurrentNoteValues(List<BaseMidiChannelEvent> messages)
        {
            foreach (var baseMessage in messages)
            {
                var noteOnMessage = baseMessage as NoteOnEvent;

                if (noteOnMessage != null)
                {
                    Bars[noteOnMessage.ChannelNumber].Value = noteOnMessage.Note;
                    Bars[noteOnMessage.ChannelNumber].IsDecaying = false;
                    continue;
                }
                
                var noteOffMessage = baseMessage as NoteOffEvent;

                if (noteOffMessage != null)
                {
                    Bars[noteOffMessage.ChannelNumber].IsDecaying = true;
                }
            }
        }

        public void ResetBars()
        {
            foreach (var equalizerBarViewModel in Bars)
            {
                equalizerBarViewModel.Value = 0;
            }
        }
    }
}
