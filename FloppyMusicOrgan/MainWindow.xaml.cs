using System.Diagnostics;
using System.IO;
using System.Windows;
using ComOutput;
using MidiParser;
using MidiParser.Entities;

namespace FloppyMusicOrgan
{
    public partial class MainWindow : Window
    {
        private ParsedMidiFile _parsedMidiFile;
        private readonly ComStreamer _comStreamer;
        private bool _isConnectedToComPort;

        public MainWindow()
        {
            InitializeComponent();
            _comStreamer = new ComStreamer();
            _comStreamer.GetAvailableComPorts();
        }

        private void BtnLoadMidi_OnClick(object sender, RoutedEventArgs e)
        {
            var midiParser = new MidiReader();
            _parsedMidiFile = midiParser.Parse(Path.Combine(
                Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
                "Resources",
                "_TestFile.mid"));
        }

        private void BtnResetDrives_OnClick(object sender, RoutedEventArgs e)
        {
            _comStreamer.SendResetDrivesCommand();
        }

        private void BtnConnectToSelectedComPort_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_isConnectedToComPort)
            {
                _isConnectedToComPort = _comStreamer.Connect("COM3");
                var midiPlayer = new MidiPlayer.MidiPlayer(_comStreamer);
                midiPlayer.Play();
            }
            else
            {
                _comStreamer.Disconnect();
                _isConnectedToComPort = false;
            }
        }
    }
}
