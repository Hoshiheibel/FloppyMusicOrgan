using System.Collections.Generic;
using FloppyMusicOrgan.Common;
using MidiParser.Entities.MidiEvents;

namespace FloppyMusicOrgan.ViewModels
{
    class EqualizerViewModel : PropertyChangedBase
    {
        public EqualizerViewModel()
        {
            Bars = new List<EqualizerBarViewModel>();

            for (int i = 0; i < 16; i++)
            {
                Bars.Add(new EqualizerBarViewModel());
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
                    continue;
                }
                
                var noteOffMessage = baseMessage as NoteOffEvent;
                
                if (noteOffMessage != null)
                    Bars[noteOffMessage.ChannelNumber].Value = 0;
            }
        }
    }
}
