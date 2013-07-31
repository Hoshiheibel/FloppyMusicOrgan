using ComOutput;

namespace MidiPlayer
{
    public class MidiPlayer
    {
        private readonly ComStreamer _comStreamer;

        public MidiPlayer(ComStreamer comStreamer)
        {
            _comStreamer = comStreamer;
        }

        public void Play()
        {
            _comStreamer.SendCommand(2, 50, 124);
        }
    }
}
