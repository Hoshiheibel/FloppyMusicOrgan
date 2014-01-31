using System.ComponentModel;
using System.Windows.Controls;
using ComOutput;
using FloppyMusicOrgan.ViewModels;
using MidiParser.Entities.MidiFile;

namespace FloppyMusicOrgan.Views
{
    public partial class FloppyMusicOrganDialog
    {
        private MidiFile _midiFile;
        private readonly ComStreamer _comStreamer;
        private bool _isConnectedToComPort;
        private FloppyMusicOrganViewModel _floppyMusicOrganViewModel;

        public FloppyMusicOrganDialog()
        {
            InitializeComponent();
            _floppyMusicOrganViewModel = new FloppyMusicOrganViewModel();
            DataContext = _floppyMusicOrganViewModel;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _floppyMusicOrganViewModel.Quit();
        }

        private void ComPort_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _floppyMusicOrganViewModel.SelectedComPortChanged();
        }
    }
}
