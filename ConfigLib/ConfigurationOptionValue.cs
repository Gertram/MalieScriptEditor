using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace ConfigLib
{
    public class ConfigurationOptionValue<T, U> : IOptionValue<T> where U : IConfigurationOptionType<T>, new()
    {
        private readonly IOptionStorage storage;
        private readonly T defaultValue;
        private readonly string name;
        public ConfigurationOptionValue(IOptionStorage storage, string name, T defaultValue = default)
        {
            this.storage = storage;
            this.name = name;
            this.defaultValue = defaultValue;
        }
        public T Value
        {
            get
            {

                var settings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).AppSettings.Settings;

                if (settings[name] == null)
                {
                    return defaultValue;
                }
                var value = settings[name].Value;
                return new U().FromString(value);
            }

            set
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[name] == null)
                {
                    settings.Add(name, new U().ToString(value));
                }
                else
                {
                    settings[name].Value = new U().ToString(value);
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                storage.NotifyPropertyChanged(name);
            }
        }
    }
}
