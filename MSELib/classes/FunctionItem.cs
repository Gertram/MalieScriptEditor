namespace MSELib.classes
{
    public class FunctionItem
    {
        public string Name { get; set; }
        public uint Id { get; set; }
        public int VMCodeOffset { get; set; }
        public uint Reserved0 { get; set; }
        public CommandItem Command { get; set; }
    }
}
