using System.ComponentModel;
using System.Windows.Controls;
using FloppyMusicOrgan.ViewModels;

namespace FloppyMusicOrgan.Views
{
    public partial class FloppyMusicOrganDialog
    {
        private readonly FloppyMusicOrganViewModel _floppyMusicOrganViewModel;

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

        private void ComPortMusicOutput_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _floppyMusicOrganViewModel.SelectedComPortForMusicOutputChanged();
        }
    }
}
