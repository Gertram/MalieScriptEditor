using System.ComponentModel;
using System.Windows;
using MSELib;

namespace MSEGui
{
    public class ContentsListItem : INotifyPropertyChanged
    {
        private bool onlyJapanese;

        public ContentsListItem(int index,StringsItem title, StringsItem value)
        {
            Index = index;
            Title = title;
            Value = value;
            Value.PropertyChanged += Value_PropertyChanged;
            Title.PropertyChanged += Value_PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(StringsItem.Text))
            {
                PropertyChanged?.Invoke(this, e);
            }
        }

        public int Index { get; }
        public StringsItem Value { get; }
        public StringsItem Title { get; }
        public bool OnlyJapanese
        {
            get => onlyJapanese;
            set
            {
                onlyJapanese = value;
                if (!onlyJapanese)
                {
                    Visibile = Visibility.Visible;
                }
                else if (Value.Text.ContainsJapanese())
                {
                    Visibile = Visibility.Visible;
                }
                else
                {
                    Visibile = Visibility.Collapsed;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Visibile)));
            }
        }
        public Visibility Visibile { get; set; } = Visibility.Visible;
    }
}