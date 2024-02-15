using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Markup;

namespace MSELib
{
    public enum StringType
    {
        Voice,
        Text
    }
    public class Dat
    {
        public int Magic { get; set; }
        public List<TitleItem> TitleItems { get; set; }
        public List<ContentItem> ContentItems { get; set; }
        public byte[] Raw { get; set; }
        public List<LineItem> Strings { get; set; }
        private void ReadTitles(BinaryReader reader)
        {
            TitleItems = new List<TitleItem>();
            bool is_continue = true;
            while (is_continue)
            {
                var startIndex = reader.BaseStream.Position;
                if (startIndex == 0x3772)
                {

                }
                var strLength = reader.ReadInt16();
                var key = reader.ReadUInt16();
                if (key != 0x8000)
                {
                    break;
                }
                var bytes = reader.ReadBytes(strLength);
                var text = Encoding.Unicode.GetString(bytes).TrimEnd('\0');
                var start = reader.BaseStream.Position;
                var parametersCount = 0;
                for (uint t; reader.BaseStream.Position < reader.BaseStream.Length; parametersCount++)
                {
                    t = reader.ReadUInt32();
                    if (t >> 24 == 0x80)
                    {
                        break;
                    }
                    if ((t & 0xFFFF) == 0x25a0)
                    {
                        is_continue = false;
                        break;
                    }
                }
                reader.BaseStream.Position = start;

                var parameters = new List<int>();
                for (int i = 0; i < parametersCount; i++)
                {
                    parameters.Add(reader.ReadInt32());
                }


                TitleItems.Add(new TitleItem
                {
                    Offset = (uint)start,
                    Title = text,
                    Parameters = parameters
                }
                );
            };
        }
        private void ReadContents(BinaryReader reader)
        {
            bool is_continue = true;
            ContentItems = new List<ContentItem>();
            reader.BaseStream.Position += sizeof(ushort);
            while (is_continue)
            {
                var start = reader.BaseStream.Position;
                var texts = new List<string>();
                while (true)
                {
                    var temp = reader.ReadUInt16();
                    if (temp == 0)
                    {
                        var end = reader.BaseStream.Position;
                        reader.BaseStream.Position = start;
                        var bytes = reader.ReadBytes((int)(end - start));
                        var text = Encoding.Unicode.GetString(bytes).TrimEnd('\0');
                        texts.Add(text);
                        if (text == "シナリオ終了\n")
                        {
                            is_continue = false;
                            break;
                        }
                        start = reader.BaseStream.Position;
                    }
                    if (temp == 0x25a0)
                    {
                        break;
                    }
                }
                ContentItems.Add(new ContentItem
                {
                    Title = new StringsItem(texts[0],false),
                    Offset = (uint)start,
                    Texts = new List<StringsItem>(texts.Skip(1).Select(x=>new StringsItem(x,false)))
                });
            };
        }
        private void ReadRaw(BinaryReader reader)
        {
            var rawLength = reader.ReadInt32();
            Raw = reader.ReadBytes(rawLength);
        }
        private void ReadStrings(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            var ranges = new List<(int start, int length)>();
            for (int i = 0; i < count; i++)
            {
                var start = reader.ReadInt32();
                var length = reader.ReadInt32();
                ranges.Add((start, length));
            }
            var tableLength = reader.ReadInt32();
            var startOffset = reader.BaseStream.Position;
            Strings = new List<LineItem>();

            for (int i = 0; i < count; i++)
            {
                var offset = ranges[i].start;
                var length = ranges[i].length;
                reader.BaseStream.Position = startOffset + offset;
                var bytes = reader.ReadBytes(length);
                var text = new LineItem(Encoding.Unicode.GetString(bytes));

                Strings.Add(text);
            }
        }
        public Dat(string filename) 
        {
            byte[] data = File.ReadAllBytes(filename);

            using(var reader = new BinaryReader(new MemoryStream(data)))
            {
                Magic = reader.ReadInt32();
                ReadTitles(reader);
                ReadContents(reader);
                ReadRaw(reader);
                ReadStrings(reader);
            }
        }
        public void WriteTitles(BinaryWriter writer)
        {
            foreach(var titleItem in TitleItems)
            {
                var title = titleItem.Title + "\0";
                var bytes = Encoding.Unicode.GetBytes(title);
                writer.Write((ushort)bytes.Length);
                writer.Write((ushort)0x8000);
                writer.Write(bytes);
                foreach(var parameter in titleItem.Parameters)
                {
                    writer.Write(parameter);
                }
            }
        }
        public void WriteContents(BinaryWriter writer)
        {
            foreach(var contentItem in ContentItems)
            {
                writer.Write((ushort)0x25a0);
                foreach(var stringItem in contentItem.Texts.Prepend(contentItem.Title))
                {
                    var line = stringItem.Text + "\0";
                    var bytes = Encoding.Unicode.GetBytes(line);
                    writer.Write(bytes);
                }
            }
        }
        public void WriteRaw(BinaryWriter writer)
        {
            writer.Write(Raw.Length);
            writer.Write(Raw);
        }
        public void WriteStrings(BinaryWriter writer)
        {
            writer.Write(Strings.Count);
            var offset = 0;
            var lines = new List<byte[]>();
            foreach (var lineItem in Strings)
            {
                writer.Write(offset);
                var line = Encoding.Unicode.GetBytes(lineItem.Dump()+'\0');
                lines.Add(line);
                writer.Write(line.Length-sizeof(ushort));
                offset += line.Length;
            }
            writer.Write(offset);
            foreach(var line in lines)
            {
                writer.Write(line);
            }
        }
        public void Save(string fileName)
        {
            using(var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Magic);
                WriteTitles(writer);
                WriteContents(writer);
                WriteRaw(writer);
                WriteStrings(writer);
                File.WriteAllBytes(fileName, stream.ToArray());
            }
        }
    }
}
