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

            LoadSettings();
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
        public double MaximumSliderPosition { get; set; }
        public double CurrentSliderPosition { get; set; }
        public string CurrentTimePosition { get; set; }
        public string TotalTime { get; set; }

        public ICommand PlayCommand { get; set; }
        public ICommand ResetDrivesCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
        public ICommand LoadMidiFileCommand { get; set; }
        public ICommand PauseCommand { get; set; }
        public ICommand TimePositionSlider_MouseLeftButtonDown { get; set; }
        public ICommand TimePositionSlider_MouseLeftButtonUp { get; set; }
        public ICommand TimePositionSlider_ValueChanged { get; set; }

        private readonly IShow _show;
        private bool _isConnectedToComPort;
        private ComStreamer _comStreamer;
        private bool _isFileLoaded;
        private MidiFile _midiFile;
        private Player _midiPlayer;
        private bool _isPlayingFile;
        private Parser _parser;
        private bool _isPaused;
        private ConvertedMidiTrack _convertedMidiFile;
        private bool _allowUpdateSliderAutomatically;

        public void Quit()
        {
            _midiPlayer.StopPlayback();
            
            if (_midiPlayer.TimePositionChanged != null)
                _midiPlayer.TimePositionChanged -= MidiPlayer_TimePositionChanged;
            
            _comStreamer.SendStopCommand();
            _comStreamer.Disconnect();
            _comStreamer.Dispose();
            _comStreamer = null;

            SaveSettings();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default["ComPort"] = SelectedComPort;
            Properties.Settings.Default.Save();
        }

        private void LoadSettings()
        {
            SelectedComPort = Properties.Settings.Default["ComPort"].ToString();
        }

        private void PrepareCommands()
        {
            PlayCommand = new DelegateCommand(x => TogglePlayFile());
            ResetDrivesCommand = new DelegateCommand(x => ResetDrives());
            ConnectCommand = new DelegateCommand(x => ToggleComPortConnection());
            LoadMidiFileCommand = new DelegateCommand(x => LoadFile());
            PauseCommand = new DelegateCommand(x => PausePlayback());
            TimePositionSlider_MouseLeftButtonDown = new DelegateCommand(x => OnTimePositionSlider_MouseLeftButtonDown());
            TimePositionSlider_MouseLeftButtonUp = new DelegateCommand(x => OnTimePositionSlider_MouseLeftButtonUp());
            TimePositionSlider_ValueChanged = new DelegateCommand(x => OnTimePositionSlider_ValueChanged());
        }

        private void OnTimePositionSlider_ValueChanged()
        {
            if (!_allowUpdateSliderAutomatically)
                _midiPlayer.GoToSelectedSongPosition(CurrentSliderPosition * 1000);
        }

        private void OnTimePositionSlider_MouseLeftButtonUp()
        {
            _allowUpdateSliderAutomatically = true;
        }

        private void OnTimePositionSlider_MouseLeftButtonDown()
        {
            _allowUpdateSliderAutomatically = false;
        }

        private void PausePlayback()
        {
            _isPaused = true;
            _midiPlayer.PausePlayback();
            ToggleButtons();
        }

        private void PrepareButtons()
        {
            IsConnectButtonEnabled = true;
            IsPlayButtonEnabled = false;
            ConnectButtonCaption = "Connect";
            PlayButtonCaption = "Play";

            CurrentSliderPosition = 0;
            MaximumSliderPosition = 100;
            CurrentTimePosition = "00:00";
            TotalTime = "00:00";

            _allowUpdateSliderAutomatically = true;
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
            _midiPlayer.TimePositionChanged += MidiPlayer_TimePositionChanged;
        }

        private void MidiPlayer_TimePositionChanged(object sender, TimePositionChangedEventArgs timePositionChangedEventArgs)
        {
            if (!_allowUpdateSliderAutomatically)
                return;

            CurrentSliderPosition = timePositionChangedEventArgs.NewDeltaTimePosition / 1000;
            CurrentTimePosition = timePositionChangedEventArgs.NewTimePosition.ToString(@"mm\:ss");
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
                _midiPlayer.Play(_convertedMidiFile);
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
            _convertedMidiFile = new TrackConverter().Convert(_midiFile);
            _isFileLoaded = true;
            ToggleButtons();
            SetupSliderForNewSong();
        }

        private void SetupSliderForNewSong()
        {
            CurrentTimePosition = "00:00";
            TotalTime =
                _convertedMidiFile.MessageList[_convertedMidiFile.MessageList.Count - 1].AbsoluteTimePosition.ToString(@"mm\:ss");

            CurrentSliderPosition = 0;
            MaximumSliderPosition =
                _convertedMidiFile.MessageList[_convertedMidiFile.MessageList.Count - 1].AbsoluteDeltaTimePosition / 1000;
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
