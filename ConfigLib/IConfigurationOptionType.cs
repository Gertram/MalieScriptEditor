namespace ConfigLib
{
    public interface IConfigurationOptionType<T>
    {
        T FromString(string value);
        string ToString(T value);
    }
}
