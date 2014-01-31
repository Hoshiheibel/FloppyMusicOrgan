using Microsoft.Win32;

namespace FloppyMusicOrgan.Infrastructure
{
    public class Show : IShow
    {
        public string FileSelection()
        {
            var dialog = new OpenFileDialog
            {
                CheckPathExists = true,
                CheckFileExists = true,
                DefaultExt = string.Join("|", "Midi files | *.mid"),
                Filter = "Midi files | *.mid",
                RestoreDirectory = true
            };

            var dialogResult = dialog.ShowDialog();

            return dialogResult.Value
                ? dialog.FileName
                : string.Empty;
        }
    }
}
