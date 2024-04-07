using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSELib
{
    public static class StringHelpers
    {
        static StringHelpers()
        {
            separators = new Dictionary<char, string>();

            separators.Add('\t', "\\t");
            separators.Add('\r', "\\r");
            separators.Add('\a', "\\a");
            separators.Add('\b', "\\b");
            separators.Add('\n', "\\n");
            separators.Add('\u0000', "\\0");
            separators.Add('\u0001', "\\1");
            separators.Add('\u0002', "\\2");
            separators.Add('\u0003', "\\3");
            separators.Add('\u0004', "\\4");
            separators.Add('\u0005', "\\5");
            separators.Add('\u0006', "\\6");
            //separators.Add('\u0007', "\\u0007");
        }
        private static Dictionary<char, string> separators;
        //public static string EscapeSeparators(this string text)
        //{
        //    var builder = new StringBuilder();

        //    foreach(var character in text)
        //    {
        //        if (separators.TryGetValue(character,out var value))
        //        {
        //            builder.Append(value);
        //            continue;
        //        }
        //        builder.Append(character);
        //    }

        //    return builder.ToString();
        //}
        //public static string UnscapeSeparators(this string text)
        //{

        //    return text;
        //}
        public static string Escape(this string text)
        {
            if (text.Length == 0)
            {
                return "[EMPTY]";
            }
            if (text == "　")
            {
                return "[EMPTY_LINE]";
            }
            var builder = new StringBuilder();

            foreach (var character in text)
            {
                if (separators.TryGetValue(character, out var value))
                {
                    builder.Append(value);
                    continue;
                }
                builder.Append(character);
            }

            return builder.ToString();
            //text = text.Replace("\r", "\\r").Replace("\n", "\\n");
            //return text;
        }
        public static string Unescape(this string text)
        {
            if(text == "[EMPTY]")
            {
                return "";
            }
            if(text == "[EMPTY_LINE]")
            {
                return "　";
            }
            foreach (var pair in separators)
            {
                text = text.Replace(pair.Value, pair.Key.ToString());
            }
            //text = text.Replace("\\r", "\r").Replace("\\n", "\n");
            return text;
        }
        private static int GetCharsCount(string text, int min, int max)
        {
            return text.Where(e => e >= min && e <= max).Count();
        }

        public static bool ContainsJapanese(this string text, int limit = 5)
        {
            if (text == null)
            {
                return false;
            }
            var hiragana = GetCharsCount(text, 0x3040, 0x309F);
            var katakana = GetCharsCount(text, 0x30A0, 0x30FF);
            var kanji = GetCharsCount(text, 0x4E00, 0x9FBF);
            return hiragana + katakana + kanji > limit;
        }
    }
}