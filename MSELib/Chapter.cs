using System.Collections.Generic;

namespace MSELib
{
    public class Chapter
    {
        public Chapter(string title, IReadOnlyList<ChapterString> strings, int start, int end)
        {
            Title = title;
            Strings = strings;
            Start = start;
            End = end;
        }

        public string Title { get; }
        public IReadOnlyList<ChapterString> Strings { get; }
        public int Start { get; }
        public int End { get; }
    }
}
