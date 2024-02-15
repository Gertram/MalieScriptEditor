namespace ConfigLib
{
    public class OptionValue<T> : IOptionValue<T>
    {
        private readonly IOptionStorage storage;
        private T _value;
        private readonly string name;
        public OptionValue(IOptionStorage storage, string name, T value = default)
        {
            this.storage = storage;
            this.name = name;
            _value = value;
        }
        public T Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
                storage.NotifyPropertyChanged(name);
            }
        }
    }
}
