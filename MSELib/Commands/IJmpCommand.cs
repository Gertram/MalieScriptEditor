namespace MSELib
{
    public interface IJmpCommand
    {
        uint CommandOffset { get; }
        BaseCommand Command { get; set; }
    }
}
