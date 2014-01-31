using System.Collections.ObjectModel;
using System.Windows.Input;
using ComOutput;
using FloppyMusicOrgan.Infrastructure;
using MidiParser;
using MidiParser.Entities.MidiFile;
using MidiPlayer;
using MidiToArduinoConverter;

namespace FloppyMusicOrgan.ViewModels
{
    class FloppyMusicOrganViewModel : PropertyChangedBase
    {
        public FloppyMusicOrganViewModel()
        {
            PrepareCommands();
            PrepareButtons();
            PrepareComStreamer();
            GetAvailableComPorts();
            PrepareMidiPlayer();
            ToggleButtons();
            _show = new Show();
        }

        public bool IsConnectButtonEnabled { get; set; }
        public bool IsPlayButtonEnabled { get; set; }
        public bool IsResetDrivesButtonEnabled { get; set; }
        public bool IsComPortSelectionEnabled { get; set; }
        public bool IsPauseButtonEnabled { get; set; }
        public string ConnectButtonCaption { get; set; }
        public string PlayButtonCaption { get; set; }
        public ObservableCollection<string> AvailableComPorts { get; set; }
        public string SelectedComPort { get; set; }

        public ICommand PlayCommand { get; set; }
        public ICommand ResetDrivesCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
        public ICommand LoadMidiFileCommand { get; set; }
        public ICommand PauseCommand { get; set; }

        private bool _isConnectedToComPort;
        private ComStreamer _comStreamer;
        private bool _isFileLoaded;
        private MidiFile _midiFile;
        private Player _midiPlayer;
        private bool _isPlayingFile;
        private IShow _show;
        private Parser _parser;
        private bool _isPaused;

        public void Quit()
        {
            _midiPlayer.StopPlayback();
            _comStreamer.SendStopCommand();
            _comStreamer.Disconnect();
            _comStreamer.Dispose();
        }

        private void PrepareCommands()
        {
            PlayCommand = new DelegateCommand(x => TogglePlayFile());
            ResetDrivesCommand = new DelegateCommand(x => ResetDrives());
            ConnectCommand = new DelegateCommand(x => ToggleComPortConnection());
            LoadMidiFileCommand = new DelegateCommand(x => LoadFile());
            PauseCommand = new DelegateCommand(x => PausePlayback());
        }

        private void PausePlayback()
        {
            _isPaused = true;
            _midiPlayer.StopPlayback();
            ToggleButtons();
        }

        private void PrepareButtons()
        {
            IsConnectButtonEnabled = true;
            IsPlayButtonEnabled = false;
            ConnectButtonCaption = "Connect";
            PlayButtonCaption = "Play";
        }

        private void PrepareComStreamer()
        {
            _comStreamer = new ComStreamer();
        }

        private void GetAvailableComPorts()
        {
            AvailableComPorts = new ObservableCollection<string>(_comStreamer.AvailableComPorts);
        }
        
        private void PrepareMidiPlayer()
        {
            _midiPlayer = new Player(_comStreamer);
        }

        private void ToggleComPortConnection()
        {
            if (!_isConnectedToComPort)
            {
                _isConnectedToComPort = _comStreamer.Connect(SelectedComPort);
                ToggleButtons();
            }
            else
            {
                _comStreamer.Disconnect();
                _isConnectedToComPort = false;
                ToggleButtons();
            }
        }

        private void ResetDrives()
        {
            if (_comStreamer.IsConnected)
                _comStreamer.SendResetDrivesCommand();
        }

        private void TogglePlayFile()
        {
            if (_isPlayingFile && !_isPaused)
            {
                _midiPlayer.StopPlayback();
                _isPaused = false;
                _isPlayingFile = false;
            }
            else if (!_isPaused)
            {
                _midiPlayer.Play(new TrackConverter().Convert(_midiFile));
                _isPlayingFile = true;
            }
            else
            {
                _midiPlayer.ResumePlayback();
                _isPaused = false;
            }

            ToggleButtons();
        }

        private void LoadFile()
        {
            var fileName = _show.FileSelection();

            if (string.IsNullOrEmpty(fileName))
                return;

            if (_parser == null)
                _parser = new Parser();

            _midiFile = _parser.Parse(fileName);
            _isFileLoaded = true;
            ToggleButtons();
        }

        private void ToggleButtons()
        {
            IsPlayButtonEnabled = _isConnectedToComPort && _isFileLoaded;
            IsPauseButtonEnabled = _isPlayingFile && !_isPaused;
            IsResetDrivesButtonEnabled = _isConnectedToComPort;
            IsConnectButtonEnabled = SelectedComPort != null;
            IsComPortSelectionEnabled = !_isConnectedToComPort;
            
            ConnectButtonCaption = _isConnectedToComPort
                ? "Disconnect"
                : "Connect";

            PlayButtonCaption = _isPlayingFile && !_isPaused
                ? "Stop"
                : "Play";
        }
        
        public void SelectedComPortChanged()
        {
            ToggleButtons();
        }
    }
}
