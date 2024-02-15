using System.ComponentModel;
using MSELib;

namespace MSEGui
{
    public class StringsListItem : INotifyPropertyChanged
    {
        public StringsListItem(int index, StringsItem value)
        {
            Index = index;
            Value = value;
            value.PropertyChanged += Value_PropertyChanged;
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
    }
}