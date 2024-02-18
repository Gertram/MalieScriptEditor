namespace MSELib.classes
{
    public class ArgumentItem
    {
        public int Offset { get; set; }
        public ArgumentType Type;
        public byte ByteValue { get; set; }
        public ushort UshortValue { get; set; }
        public uint UintValue { get; set; }
    }
}
