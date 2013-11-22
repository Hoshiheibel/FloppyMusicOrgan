using System.Collections.ObjectModel;
using System.Windows.Input;
using ComOutput;
using FloppyMusicOrgan.Infrastructure;
using MidiParser.Entities.MidiFile;
using MidiPlayer;

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
        }

        public bool IsConnectButtonEnabled { get; set; }
        public bool IsPlayButtonEnabled { get; set; }
        public bool IsResetDrivesButtonEnabled { get; set; }
        public bool IsComPortSelectionEnabled { get; set; }
        public string ConnectButtonCaption { get; set; }
        public string PlayButtonCaption { get; set; }
        public ObservableCollection<string> AvailableComPorts { get; set; }
        public string SelectedComPort { get; set; }

        public ICommand PlayCommand { get; set; }
        public ICommand ResetDrivesCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
        public ICommand LoadMidiFileCommand { get; set; }
        public ICommand LoadMidiFileButtonEnabled { get; set; }

        private bool _isConnectedToComPort;
        private ComStreamer _comStreamer;
        private bool _isFileLoaded;
        private MidiFile _midiFile;
        private Player _midiPlayer;
        private bool _isPlayingFile;

        public void Quit()
        {
            _midiPlayer.StopPlayback();
            _comStreamer.SendStopCommand();
            _comStreamer.Disconnect();
            _comStreamer.Dispose();
        }

        private void PrepareCommands()
        {
            PlayCommand = new DelegateCommand(x => PlayFile());
            ResetDrivesCommand = new DelegateCommand(x => ResetDrives());
            ConnectCommand = new DelegateCommand(x => ConnectToComPort());
            LoadMidiFileCommand = new DelegateCommand(x => LoadFile());
            LoadMidiFileButtonEnabled = new DelegateCommand(x => LoadFile());
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

        private void ConnectToComPort()
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
            _comStreamer.SendResetDrivesCommand();
        }

        private void PlayFile()
        {
            ToggleButtons();

            if (_isPlayingFile)
                _midiPlayer.StopPlayback();
            else
                _midiPlayer.Play(_midiFile);

            _isPlayingFile = !_isPlayingFile;
        }

        private void LoadFile()
        {
            var midiParser = new MidiParser.MidiParser();
            _midiFile = midiParser.Parse(@"E:\Floppy\_gp_v10.mid");
            _isFileLoaded = true;
            ToggleButtons();
        }

        private void ToggleButtons()
        {
            IsPlayButtonEnabled = _isConnectedToComPort && _isFileLoaded;
            IsResetDrivesButtonEnabled = _isConnectedToComPort;
            IsConnectButtonEnabled = SelectedComPort != null;
            IsComPortSelectionEnabled = !_isConnectedToComPort;
            
            ConnectButtonCaption = _isConnectedToComPort
                ? "Disconnect"
                : "Connect";

            PlayButtonCaption = _isPlayingFile
                ? "Stop"
                : "Play";
        }
        
        public void SelectedComPortChanged()
        {
            ToggleButtons();
        }
    }
}
