namespace ConfigLib
{
    public interface IOptionValue<T>
    {
        T Value { get; set; }
    }
}
