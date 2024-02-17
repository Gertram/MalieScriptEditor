using System.ComponentModel;

namespace MSELib
{
    public class StringsItem : INotifyPropertyChanged
    {
        private string text;

        public event PropertyChangedEventHandler PropertyChanged;
        public string Text
        {
            get => text;
            set
            {
                text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }
        public StringsItem(string line,bool auto_unescape = true)
        {
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
