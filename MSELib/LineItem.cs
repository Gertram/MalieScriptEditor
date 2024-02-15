using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace MSELib
{
    public class LineItem : INotifyPropertyChanged
    {
        private static List<char> chars = new List<char> { '\t','\r','\a','\b', '\u0000','\u0001', '\u0002', '\u0003', '\u0004', '\u0005','\u0006','\u0007' };
        private List<StringsItem> texts;

        public event PropertyChangedEventHandler PropertyChanged;
        public List<StringsItem> Texts
        {
            get => texts;
            private set
            {
                texts = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StringsItem)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibleTexts)));
            }
        }
        public IEnumerable<StringsItem> VisibleTexts => Texts.Where(x => !x.IsDelimeter && x.Type == StringType.Text);
        public LineItem(string line)
        {
            line = line.Replace("\f1", "[NAME]");
            string current = "";
            bool isDelimeter = false;
            var strings = new List<StringsItem>();
            void swap(){
                if(current.Length != 0)
                {
                    strings.Add(new StringsItem(current,isDelimeter));
                    current = "";
                }
                isDelimeter = !isDelimeter;
            };
            foreach(var sym in line)
            {
                var contains = chars.Contains(sym);
                if (!isDelimeter && contains || isDelimeter && !contains)
                {
                    swap();
                }

                current += sym;
            }
            swap();
            Texts = strings;
        }
        public string Dump()
        {
            return string.Join("", Texts.Select(x => x.Text.Replace("[NAME]","\f1")));
        }
        public override string ToString()
        {
            return string.Join("\n", texts.Where(x=>!x.IsDelimeter && x.Type == StringType.Text));
        }
    }
}
