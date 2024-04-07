using System.Collections.Generic;
using System.ComponentModel;
using MSELib.classes;

namespace MSELib.classes
{
    public enum StringTag
    {
        None = 0,
        Name,
        Label,
        Chapter,
        Select
    }
    public class StringItem : INotifyPropertyChanged
    {
        private string text;
        private uint offset;

        public event PropertyChangedEventHandler PropertyChanged;
        public uint Offset
        {
            get => offset;
            set
            {
                offset = value;
            }
        }
        public string Text
        {
            get => text;
            set
            {
                text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }
        public StringTag Tag { get; set; }
        public StringItem(string line, uint offset, bool auto_unescape = true)
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
