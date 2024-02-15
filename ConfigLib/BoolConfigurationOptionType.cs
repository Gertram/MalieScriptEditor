namespace ConfigLib
{
    public class BoolConfigurationOptionType : IConfigurationOptionType<bool>
    {
        public bool FromString(string value)
        {
            return bool.Parse(value);
        }

        public string ToString(bool value)
        {
            return value.ToString();
        }
    }
}
