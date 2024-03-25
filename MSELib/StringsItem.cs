using System.Collections.Generic;
using System.ComponentModel;
using MSELib.classes;

namespace MSELib
{
    public class StringsItem : INotifyPropertyChanged
    {
        private string text;

        public event PropertyChangedEventHandler PropertyChanged;
        public int Offset { get; set; }
        public List<ArgumentItem> Arguments { get; } = new List<ArgumentItem>();
        public string Text
        {
            get => text;
            set
            {
                text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }
        public StringsItem(int offset,string line,bool auto_unescape = true)
        {
            Offset = offset;
            Text = line;
            if (auto_unescape)
            {
                Text = Text.Escape();
            }
        }
        public string Dump()
        {
            return Text.Unescape();
        }
        public override string ToString()
        {
            return Text;
        }
    }
}
