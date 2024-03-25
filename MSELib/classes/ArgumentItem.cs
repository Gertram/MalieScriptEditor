namespace MSELib.classes
{
    public class ArgumentItem
    {
        public ArgumentItem(int offset, ArgumentType type, int value)
        {
            Offset = offset;
            Type = type;
            Value = value;
        }

        public int Offset { get; }
        public CommandItem Command { get; set; }
        public ArgumentType Type { get; set; }
        public int Value { get; set; }
        public CommandItem CommandPtr { get; set; }
    }
}
