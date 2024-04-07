using System.ComponentModel;
using MSELib.classes;

namespace MSEGui
{
    public class StringListItem : INotifyPropertyChanged
    {
        public StringListItem(int index, StringItem value)
        {
            Index = index;
            Value = value;
            value.PropertyChanged += Value_PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(StringItem.Text))
            {
                PropertyChanged?.Invoke(this, e);
            }
        }

        public int Index { get; }
        public StringItem Value { get; }
    }
}