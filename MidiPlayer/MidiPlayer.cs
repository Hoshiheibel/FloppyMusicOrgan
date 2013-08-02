using System.Threading;
using ComOutput;

namespace MidiPlayer
{
    public class MidiPlayer
    {
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

        private readonly ComStreamer _comStreamer;

        public MidiPlayer(ComStreamer comStreamer)
        {
            _comStreamer = comStreamer;
        }

        public void Play()
        {
        }

        public void PlayTest()
        {
            _comStreamer.SendCommand(4, _microPeriods[66] / ArduinoResolution * 2);
            Thread.Sleep(500);
            _comStreamer.SendCommand(4, _microPeriods[68] / ArduinoResolution * 2);
            Thread.Sleep(500);
            _comStreamer.SendCommand(4, _microPeriods[70] / ArduinoResolution * 2);
            Thread.Sleep(500);
            _comStreamer.SendCommand(4, _microPeriods[72] / ArduinoResolution * 2);
            Thread.Sleep(500);
            _comStreamer.SendCommand(4, 0);
            _comStreamer.SendResetDrivesCommand();
        }
    }
}
