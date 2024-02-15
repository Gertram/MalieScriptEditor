namespace ConfigLib
{
    public class StringConfigurationOptionType : IConfigurationOptionType<string>
    {
        public string FromString(string value)
        {
            return value;
        }

        public string ToString(string value)
        {
            return value;
        }
    }
}
