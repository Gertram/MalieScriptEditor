using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace MSELib
{
    public class TitleItem : INotifyPropertyChanged
    {
        private string title;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title
        {
            get => title;
            set
            {
                title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(title)));
            }
        }
        public string OffsetHex => Offset.ToString("X");
        public uint Offset { get; set; }
        public List<int> Parameters { get; set; } = new List<int>();
        public string HexParameters => string.Join("|", Parameters.Select(x => x.ToString("X").PadLeft(4,'0')));
        public bool IsJapanese => title.ContainsJapanese();
    }
}
