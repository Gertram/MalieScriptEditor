using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace MSELib.classes
{
    public class ContentItem : INotifyPropertyChanged
    {
        private StringsItem title;
        private List<StringsItem> texts;

        public event PropertyChangedEventHandler PropertyChanged;

        public StringsItem Title
        {
            get => title;
            set
            {
                title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }
        public List<StringsItem> Texts
        {
            get => texts;
            set
            {
                texts = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Texts)));
            }
        }
        public string OffsetHex => Offset.ToString("X");
        public uint Offset { get; set; }
        public bool IsJapanese => texts.Any(x=>x.Text.ContainsJapanese());
    }
}
