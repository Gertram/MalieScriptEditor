using Microsoft.Win32;
using MSELib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace MSEGui.IO
{
    internal class SoManyStringsException : Exception
    {
        public int LineIndex { get; }
        public int MaxStringsCount { get; }
        public SoManyStringsException(int lineIndex, int maxStringsCount) : base($"So many strings in line: {lineIndex}.\nMax string count for it {maxStringsCount}")
        {
            LineIndex = lineIndex;
            MaxStringsCount = maxStringsCount;
        }
    }
    internal class SoManyLinesException : Exception
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
    internal class ContentItemNotFound : Exception
    {
        public int LineIndex { get; }
        public string Title { get; }
        public ContentItemNotFound(string title, int lineIndex) : base($"ContentItem \"{title}\" not found in line: {lineIndex}.")
        {
            Title = title;
            LineIndex = lineIndex;
        }
    }
    internal class ContentItemNotAll : Exception
    {
        public int LineIndex { get; }
        public ContentItemNotAll(int lineIndex) : base($"ContentItem not all items was loaded in line: {lineIndex}.")
        {
            LineIndex = lineIndex;
        }
    }
    internal static class TXTUtil
    {
        private static void LoadStrings(StreamReader reader,List<StringsItem> texts,ref int lineIndex, bool allow_new = false)
        {
            var index = 0;

            for (; !reader.EndOfStream; index++)
            {
                var line = reader.ReadLine();
                lineIndex++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    if(!allow_new && index != texts.Count)
                    {
                        throw new ContentItemNotAll(lineIndex);
                    }
                    return;
                }

                if (index >= texts.Count)
                {
                    if (!allow_new)
                    {
                        throw new SoManyStringsException(lineIndex - 1, index);
                    }
                    else
                    {
                        texts.Add(new StringsItem(0,line, false));
                    }
                }
                else
                {

                    texts[index].Text = line;
                }
            }
        }
        public static void ImportOthers(MSEScript script, string filename)
        {
            using (var reader = new StreamReader(new BufferedStream(File.OpenRead(filename))))
            {
                var lineIndex = 1;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    lineIndex++;
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var contentItem = script.ContentItems.FirstOrDefault(x => x.Title.Text == line);
                    if (contentItem == null)
                    {   
                        throw new ContentItemNotFound(line,lineIndex-1);
                    }
                    LoadStrings(reader, contentItem.Texts, ref lineIndex);
                }
            }
        }
       
        public static void ImportStrings(MSEScript script, string filename)
        {
            using (var reader = new StreamReader(new BufferedStream(File.OpenRead(filename))))
            {
                var lineIndex = 1;
                var lineItemIndex = 0;
                for (; !reader.EndOfStream; lineItemIndex++)
                {
                    if(lineItemIndex >= script.Strings.Count)
                    {
                        throw new SoManyLinesException(lineIndex, lineItemIndex+1, script.Strings.Count);
                    }
                    LoadStrings(reader, script.Strings[lineItemIndex].Texts, ref lineIndex,true);
                }
            }
        }
    }
}
