using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using ComOutput;
using MidiParser;
using MidiParser.Entities;

namespace FloppyMusicOrgan
{
    public partial class MainWindow
    {
        private ParsedMidiFile _parsedMidiFile;
        private readonly ComStreamer _comStreamer;
        private bool _isConnectedToComPort;
        private bool _isFileLoaded;

        public MainWindow()
        {
            InitializeComponent();
            _comStreamer = new ComStreamer();
            _comStreamer.GetAvailableComPorts();
        }

        private void BtnLoadMidi_OnClick(object sender, RoutedEventArgs e)
        {
            var midiParser = new MidiParser.MidiParser();
            ////_parsedMidiFile = midiParser.Parse(Path.Combine(
            ////    Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
            ////    "Resources",
            ////    "_TestFile.mid"));
            _parsedMidiFile = midiParser.Parse(@"E:\Floppy\happybirthday_01.mid");
            _isFileLoaded = true;
            ToggleButtons();
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
                ToggleButtons();
                btnConnectToSelectedComPort.Content = "Disconnect";
            }
            else
            {
                _comStreamer.Disconnect();
                _isConnectedToComPort = false;
                ToggleButtons();
                btnConnectToSelectedComPort.Content = "Connect";
            }
        }

        private void ToggleButtons()
        {
            btnPlay.IsEnabled = _isConnectedToComPort && _isFileLoaded;
            btnResetDrives.IsEnabled = _isConnectedToComPort;
        }

        private void BtnPlayTest_OnClick(object sender, RoutedEventArgs e)
        {
            var midiPlayer = new MidiPlayer.MidiPlayer(_comStreamer);
            midiPlayer.Play(_parsedMidiFile.ConvertedTrack);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (_isConnectedToComPort)
                _comStreamer.Disconnect();

            _comStreamer.Dispose();
        }
    }
}
