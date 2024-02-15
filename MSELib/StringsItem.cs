using System.ComponentModel;

namespace MSELib
{
    public class StringsItem : INotifyPropertyChanged
    {
        private string text;

        public event PropertyChangedEventHandler PropertyChanged;
        public string ClearedText => Text.Replace("\n", "");
        public string Text
        {
            get => text;
            set
            {
                text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClearedText)));
            }
        }
        public StringsItem(string line, bool isDelimeter)
        {
            Text = line;
            IsDelimeter = isDelimeter;
            if (!IsDelimeter)
            {
                if (line.StartsWith("v_"))
                {
                    Type = StringType.Voice;
                }
                else
                {
                    Type = StringType.Text;
                }
            }
        }
        public bool IsDelimeter { get; private set; }
        public StringType Type { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }
}
