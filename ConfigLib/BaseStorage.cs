using System;
using System.ComponentModel;

namespace ConfigLib
{
    public abstract class BaseStorage : BasePropertyChanged, IOptionStorage, INotifyPropertyChanged, ICloneable
    {
        public ISaveStorageProvider SaveStorageProvider { private get; set; }
        public abstract object Clone();

        public void NotifyPropertyChanged(string name)
        {
            Notify(name);
            SaveStorageProvider?.Save(this);
        }
        protected IOptionValue<T> GetOption<T>(string name, T value = default)
        {
            return new OptionValue<T>(this, name, value);
        }
    }
}
