using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace MSELib
{
    public class LineItem : INotifyPropertyChanged
    {
        private static List<char> chars = new List<char> { '\t','\r','\a','\b', '\u0000','\u0001', '\u0002', '\u0003', '\u0004', '\u0005','\u0006','\u0007' };
        private List<StringsItem> texts;

        public event PropertyChangedEventHandler PropertyChanged;
        public string Voice { get; set; }
        public List<StringsItem> Texts
        {
            get => texts;
            private set
            {
                texts = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StringsItem)));
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
            if (line.StartsWith("　そして、その中に、私のお墓があったことも……"))
            {

            }
            Texts = line.Split('\n').Select(x => new StringsItem(x)).ToList();
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
        public override string ToString()
        {
            return string.Join("\n", texts);
        }
    }
}
