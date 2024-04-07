using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Reflection;
using System;

namespace MSELib.classes
{
    public class LineItem : INotifyPropertyChanged
    {
        private static readonly List<char> chars = new List<char> { '\t', '\r', '\a', '\b', '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007' };
        private List<StringItem> texts;

        public event PropertyChangedEventHandler PropertyChanged;
        public string Voice { get; set; }
        public List<StringItem> Texts
        {
            get => texts;
            private set
            {
                texts = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StringItem)));
            }
        }
        public string End { get; private set; }
        public LineItem(string line)
        {
            line = line.Replace("\a\f1\0", "[NAME]");
            var regex = new Regex(@"\a\u0008(?<voice>v_.*\d+)\0(?<content>.*)",RegexOptions.Singleline);
            var match = regex.Match(line);
            if (match.Success)
            {
                Voice = match.Groups["voice"].Value;
                line = match.Groups["content"].Value;
            }
            var startPosition = line.Length - 1;
            for (; startPosition >= 0; startPosition--)
            {
                if (!chars.Contains(line[startPosition]))
                {
                    break;
                }
            }
            if(startPosition != line.Length - 1)
            {
                End = line.Substring(startPosition+1);
                line = line.Substring(0, startPosition+1);
            }
            else
            {
                End = "";
            }
            Texts = line.Split('\n').Select(x => new StringItem(x,0)).ToList();
        }
        private string GetVoicePath()
        {
            if(Voice == null)
            {
                return "";
            }
            return $"\a\b{Voice}\0";
        }
        public string Dump()
        {
            return GetVoicePath()+string.Join("\n", Texts.Select(x => x.Dump().Replace("[NAME]", "\a\f1\0"))) + End;
        }
        public void Update(IEnumerable<string> lines)
        {
            var i = 0;
            foreach(var line in lines)
            {
                if (i >= texts.Count)
                {
                    texts.Add(new StringItem(line, 0, false));
                }
                else
                {
                    if (texts[i].Text != line)
                    {

                    }
                    texts[i].Text = line;
                }
                i++;
            }
        }
        public string Text
        {
            get => ToString();
            set {
                var lines = value.Split(new string[] { "\r\n", "\n" },StringSplitOptions.RemoveEmptyEntries);
                for(int i = 0;i < lines.Length;i++)
                {
                    var line = lines[i];
                    if (i >= texts.Count)
                    {
                        texts.Add(new StringItem(line, 0, false));
                    }
                    else
                    {

                        texts[i].Text = line;
                    }
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Texts)));
            }
        }
        public override string ToString()
        {
            return string.Join("\n", texts);
        }
    }
}
