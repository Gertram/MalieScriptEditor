using MSELib.classes;

namespace MSELib
{
    public class ChapterString
    {
        public string Name { get; }
        public LineItem Line { get; }
        public ChapterString(string name, LineItem line)
        {
            Name = name;
            Line = line;
        }
    }
}
