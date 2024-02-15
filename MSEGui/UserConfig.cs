using System.ComponentModel;
using ConfigLib;

namespace MSEGui.Config
{

    public class UserConfig : INotifyPropertyChanged
    {
        private class DTO : BaseStorage
        {
            private readonly IOptionValue<string> lastFile;
            private readonly IOptionValue<string> language;
            private readonly IOptionValue<bool> onlyJapanese;
            public string LastFile { get => lastFile.Value; set => lastFile.Value = value; }
            public string Language { get => language.Value; set => language.Value = value; }
            public bool OnlyJapanese { get => onlyJapanese.Value; set => onlyJapanese.Value = value; }
            internal DTO()
            {
                lastFile = GetOption<string, StringConfigurationOptionType>(nameof(LastFile));
                language = GetOption<string, StringConfigurationOptionType>(nameof(Language),"en-US");
                onlyJapanese = GetOption<bool, BoolConfigurationOptionType>(nameof(OnlyJapanese),false);
            }
            private IOptionValue<T> GetOption<T, U>(string name, T _default = default) where U : IConfigurationOptionType<T>, new()
            {
                return new ConfigurationOptionValue<T, U>(this, name, _default);
            }
            public override object Clone()
            {
                return new DTO
                {
                    LastFile = LastFile,
                    Language = Language
                };
            }
        }

        public string LastFile { get => Storage.LastFile; set => Storage.LastFile = value; }
        public string Language { get => Storage.Language; set => Storage.Language = value; }
        public bool OnlyJapanese { get => Storage.OnlyJapanese; set => Storage.OnlyJapanese = value; }

        private DTO Storage { get; }

        internal UserConfig()
        {
            Storage = new DTO();
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => Storage.PropertyChanged += value;

            remove => Storage.PropertyChanged -= value;
        }
    }
}
