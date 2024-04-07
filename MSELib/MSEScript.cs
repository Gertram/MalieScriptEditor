using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Markup;
using System.Runtime.InteropServices.ComTypes;
using System.Collections;
using System.Reflection.Emit;
using System.Xml.Linq;
using MSELib.classes;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;

namespace MSELib
{
    public class MSEScript
    {
        private uint Magic { get; set; }
        public List<StringItem> DataStrings { get; set; }
        public List<VarItem> Vars { get; private set; }
        public List<FunctionItem> Functions { get; private set; }
        public List<LabelItem> Labels { get; private set; }
        public List<Chapter> Chapters { get; private set; }
        public List<BaseCommand> Commands { get; private set; }
        public List<LineItem> Strings { get; private set; }
        public MSEScript(byte[] data)
        {
            var parser = new VMParser(data);
            Magic = parser.Magic;
            Vars = parser.Vars;
            Functions = parser.Functions;
            Labels = parser.Labels;
            Chapters = parser.Chapters;
            Commands = parser.Commands;
            DataStrings = parser.DataStrings;
            Strings = parser.Strings;
        }
        public MSEScript(string filename) : this(File.ReadAllBytes(filename))
        {
        }
        private void WriteVars(BinaryWriter writer)
        {
            writer.Write(Vars.Count);
            foreach (var varItem in Vars)
            {
                var bytes = Encoding.Unicode.GetBytes(varItem.Name + '\0');
                writer.Write((uint)(bytes.Length | 0x80000000));
                writer.Write(bytes);
                foreach (var parameter in varItem.Parameters)
                {
                    writer.Write(parameter);
                }
            }
        }
        private void WriteFunctions(BinaryWriter writer)
        {
            writer.Write(Magic);
            writer.Write(Functions.Count);
            foreach (var functionItem in Functions)
            {
                var bytes = Encoding.Unicode.GetBytes(functionItem.Name + '\0');
                if(writer.BaseStream.Position > 0x6a30)
                {

                }
                writer.Write((uint)(bytes.Length | 0x80000000));
                writer.Write(bytes);
                writer.Write(functionItem.Id);
                writer.Write(functionItem.Reserved0);
                writer.Write(functionItem.Command.Offset);
                //writer.Write(pair.Value.VMCodeOffset);
            }
        }
        private void WriteLabels(BinaryWriter writer)
        {
            writer.Write(Labels.Count);
            foreach (var label in Labels)
            {
                var bytes = Encoding.Unicode.GetBytes(label.Name + '\0');
                writer.Write((uint)(bytes.Length | 0x80000000));
                writer.Write(bytes);
                writer.Write(label.Command.Offset);
                //writer.Write(pair.Value.VMCodeOffset);
            }
        }
        private void WriteTitles(BinaryWriter writer)
        {
            WriteVars(writer);
            WriteFunctions(writer);
            WriteLabels(writer);
        }
        private static readonly Regex regex = new Regex(@"\a\u0001.*\n.*");
        public uint CalcDataStringsLength()
        {
            uint offset = 0;
            foreach (var stringItem in DataStrings)
            {
                stringItem.Offset = offset;
                var line = stringItem.Dump().Replace("[NAME]", "\a\f1\0") + "\0";
                var bytes = Encoding.Unicode.GetBytes(line);
                offset += (uint)bytes.Length;
                if (regex.IsMatch(line))
                {
                    offset += sizeof(ushort);
                }
            }
            return offset;
        }
        public void WriteDataStrings(BinaryWriter writer,uint dataLength)
        {
            writer.Write(dataLength);
            foreach (var stringItem in DataStrings)
            {
                var line = stringItem.Dump().Replace("[NAME]", "\a\f1\0") + "\0";
                var bytes = Encoding.Unicode.GetBytes(line);
                writer.Write(bytes);
                if (regex.IsMatch(line))
                {
                    writer.Write((ushort)0);
                }
            }
        }
        public uint CalCodeLength()
        {
            uint offset = 0;
            foreach (var command in Commands)
            {
                command.Offset = offset;
                offset += command.Length;
            }
            return offset;
        }
        public void WriteCode(BinaryWriter writer,uint codeLength)
        {
            writer.Write(codeLength);
            foreach (var command in Commands)
            {
                command.Write(writer);
            }
        }
        public void WriteStrings(BinaryWriter writer)
        {
            writer.Write(Strings.Count);
            var offset = 0;
            var lines = new List<byte[]>();
            foreach (var lineItem in Strings)
            {
                writer.Write(offset);
                var text = lineItem.Dump() + '\0';
                var line = Encoding.Unicode.GetBytes(text);
                lines.Add(line);
                writer.Write(line.Length - sizeof(ushort));
                offset += line.Length;
            }
            writer.Write(offset);
            foreach (var line in lines)
            {
                writer.Write(line);
            }
        }
        public byte[] Save()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                var dataLength = CalcDataStringsLength();
                var codeLength = CalCodeLength();
                WriteTitles(writer);
                WriteDataStrings(writer,dataLength);
                WriteCode(writer,codeLength);
                WriteStrings(writer);
                return stream.ToArray();
            }
        }
        public void Save(string fileName)
        {
            File.WriteAllBytes(fileName, Save().ToArray());
        }
    }
}
