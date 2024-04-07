using Microsoft.Win32;
using MSELib;
using MSELib.classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace MSEGui.IO
{
    public class SoManyStringsException : Exception
    {
        public int LineIndex { get; }
        public int MaxStringsCount { get; }
        public SoManyStringsException(int lineIndex, int maxStringsCount) : base($"So many strings in line: {lineIndex}.\nMax string count for it {maxStringsCount}")
        {
            LineIndex = lineIndex;
            MaxStringsCount = maxStringsCount;
        }
    }
    public class SoManyLinesException : Exception
    {
        public int LineIndex { get; }
        public int LineItemIndex { get; }
        public int MaxLineCount { get; }
        public SoManyLinesException(int lineIndex, int lineItemIndex,int maxLineCount) : base($"So many lines in line: {lineIndex}.\nMax line count for it {maxLineCount} was {lineItemIndex}.")
        {
            LineIndex = lineIndex;
            MaxLineCount = maxLineCount;
            LineItemIndex = lineItemIndex;
        }
    }
    public class ContentItemNotFound : Exception
    {
        public int LineIndex { get; }
        public string Title { get; }
        public ContentItemNotFound(string title, int lineIndex) : base($"ContentItem \"{title}\" not found in line: {lineIndex}.")
        {
            Title = title;
            LineIndex = lineIndex;
        }
    }
    public class ContentItemNotAll : Exception
    {
        public int LineIndex { get; }
        public ContentItemNotAll(int lineIndex) : base($"ContentItem not all items was loaded in line: {lineIndex}.")
        {
            LineIndex = lineIndex;
        }
    }
    public static class TextUtil
    {
        public static void ImportOthers(IReadOnlyList<StringItem> strings, StreamReader reader)
        {
           
            var stringItemIndex = 0;
            for (var lineIndex = 1; !reader.EndOfStream;lineIndex++)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                if(lineIndex == 229833)
                {

                }
                if (stringItemIndex >= strings.Count)
                {
                    throw new SoManyLinesException(lineIndex, stringItemIndex + 1, strings.Count);
                }
                strings[stringItemIndex].Text = line;
                stringItemIndex++;
            }
        }

        public static void ExportOthers(IEnumerable<StringItem> strings, StreamWriter writer)
        {
            writer.Write(string.Join("\n\n", strings.Select(x=>x.Text)), Encoding.UTF8);
        }
        private static List<string> LoadStrings(StreamReader reader, ref int lineIndex)
        {
            var lines = new List<string>();
            var index = 0;

            for (; !reader.EndOfStream; index++)
            {
                var line = reader.ReadLine();
                lineIndex++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    return lines;
                }

                var pos = line.IndexOf("⋮");
                if (pos != -1)
                {
                    if (line[pos + 1] == ' ')
                    {
                        pos++;
                    }
                    line = line.Substring(pos + 1);
                }
                lines.Add(line);
            }
            return lines;
        }
        public static void ExportChapter(IEnumerable<ChapterString> strings, StreamWriter writer)
        {
            writer.Write(string.Join("\n\n", strings
                .Select(x => (!string.IsNullOrEmpty(x.Name)?x.Name + "⋮ ":"") +x.Line.Text)), Encoding.UTF8);

        }
        public static void ImportChapter(IReadOnlyList<ChapterString> strings, StreamReader reader)
        {
            var lineIndex = 1;
            var lineItemIndex = 0;
            while (!reader.EndOfStream)
            {
                if (lineItemIndex >= strings.Count)
                {
                    throw new SoManyLinesException(lineIndex, lineItemIndex + 1, strings.Count);
                }
                var lines = LoadStrings(reader, ref lineIndex);
                if (lines.Count == 0)
                {
                    continue;
                }
                strings[lineItemIndex].Line.Update(lines);
                lineItemIndex++;
            }
        }
        public static void ExportStrings(IEnumerable<LineItem> strings, StreamWriter writer)
        {
            writer.Write(string.Join("\n\n", strings.Select(x => x.Text)), Encoding.UTF8);
        }
        public static void ImportStrings(IReadOnlyList<LineItem> strings,StreamReader reader)
        {
            var lineIndex = 1;
            var lineItemIndex = 0;
            while (!reader.EndOfStream)
            {
                if (lineItemIndex >= strings.Count)
                {
                    throw new SoManyLinesException(lineIndex, lineItemIndex + 1, strings.Count);
                }
                var lines = LoadStrings(reader, ref lineIndex);
                if (lines.Count == 0)
                {
                    continue;
                }
                strings[lineItemIndex].Update(lines);
                lineItemIndex++;
            }
        }
    }
}
