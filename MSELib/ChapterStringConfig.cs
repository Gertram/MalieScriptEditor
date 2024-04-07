namespace MSELib
{
    public class ChapterStringConfig
    {
        public ChapterStringConfig()
        {

        }
        public ChapterStringConfig(string name, uint index)
        {
            Name = name;
            Index = index;
        }

        public string Name { get; set; }
        public uint Index { get; set; }
    }
}
