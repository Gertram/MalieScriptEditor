using MSELib;
using MSELib.classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MSEGui.IO
{
    public class JsonUtil
    {
        public static void ImportOthers(IReadOnlyList<StringItem> strings, StreamReader reader)
        {
            var jsonReader = new JsonTextReader(reader);
            var tokens = JsonSerializer.CreateDefault().Deserialize<List<string>>(jsonReader);
            foreach (var (str, index) in tokens.Select((x, index) => (x, index)))
            {
                var item = strings[index];
                item.Text = str;
            }
        }
        public static void ExportOthers(IEnumerable<StringItem> strings,StreamWriter writer)
        {
            var jsonWriter = new JsonTextWriter(writer);
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Formatting = Formatting.Indented;
            JsonSerializer.CreateDefault(jsonSettings).Serialize(jsonWriter, strings.Select(x => x.Text));
        }
        public static void ExportChapter(IEnumerable<ChapterString> strings, StreamWriter writer)
        {
            var jsonWriter = new JsonTextWriter(writer);
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Formatting = Formatting.Indented;
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            JsonSerializer.CreateDefault(jsonSettings)
                .Serialize(jsonWriter, strings.Select(x => new ChapterEntry(x.Name, x.Line.Texts.Select(y => y.Text).ToList())));
        }
        class ChapterEntry
        {
            public ChapterEntry() { }
            public ChapterEntry(string name, List<string> lines)
            {
                Name = name;
                Lines = lines;
            }

            public string Name { get; set; }
            public List<string> Lines { get; set; }
        }
        public static void ImportChapter(IReadOnlyList<ChapterString> strings, StreamReader reader)
        {
            var jsonReader = new JsonTextReader(reader);
            var tokens = JsonSerializer.CreateDefault().Deserialize<List<ChapterEntry>>(jsonReader);
            foreach (var (entry, index) in tokens.Select((x, index) => (x, index)))
            {
                var item = strings[index];
                foreach (var (stringItem, ind) in item.Line.Texts.Select((x, ind) => (x, ind)))
                {
                    stringItem.Text = entry.Lines[ind] as string;
                }
            }
        }
        public static void ExportStrings(IEnumerable<LineItem> strings, StreamWriter writer)
        {
            var jsonWriter = new JsonTextWriter(writer);
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Formatting = Formatting.Indented;
            JsonSerializer.CreateDefault(jsonSettings)
                .Serialize(jsonWriter, strings.Select(x => x.Texts.Select(y => y.Text)));
        }
        public static void ImportStrings(IReadOnlyList<LineItem> strings, StreamReader reader)
        {
            var jsonReader = new JsonTextReader(reader);
            var tokens = JsonSerializer.CreateDefault().Deserialize<List<List<string>>>(jsonReader);
            foreach (var (strs, index) in tokens.Select((x, index) => (x, index)))
            {
                var item = strings[index];
                foreach (var (stringItem, ind) in item.Texts.Select((x, ind) => (x, ind)))
                {
                    stringItem.Text = strs[ind];
                }
            }
        }
    }
}
