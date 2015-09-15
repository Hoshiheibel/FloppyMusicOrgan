using FloppyMusicOrgan.Common;

namespace FloppyMusicOrgan.ViewModels
{
    public class EqualizerBarViewModel : PropertyChangedBase
    {
        public int Value { get; set; }
        public bool IsDecaying { get; set; }
    }
}
