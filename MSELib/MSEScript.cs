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

namespace MSELib
{
    public class MSEScript
    {
        public static IReadOnlyList<(CommandType, IReadOnlyList<IArgumentFactory>)> CommandsDict { get; }
        static MSEScript()
        {
            var byteArgumentType = new ArgumentFactory<ByteArgument>();
            var ushortArgumentType = new ArgumentFactory<UshortArgument>();
            var uintArgumentType = new ArgumentFactory<UintArgument>();
            var byteStrArgumentType = new ArgumentFactory<ByteStrArgument>();
            var ushortStrArgumentType = new ArgumentFactory<UshortStrArgument>();
            var uintStrArgumentType = new ArgumentFactory<UintStrArgument>();
            var byteFunctionArgumentType = new ArgumentFactory<ByteFunctionArgument>();
            var uintFunctionArgumentType = new ArgumentFactory<UintFunctionArgument>();


            CommandsDict = new List<(CommandType, IReadOnlyList<IArgumentFactory>)>
            {
                (CommandType.JMP, new List<IArgumentFactory>() { uintArgumentType } ),
                (CommandType.JNZ, new List<IArgumentFactory>() { uintArgumentType } ),
                (CommandType.JZ, new List<IArgumentFactory>() {uintArgumentType } ),
                (CommandType.CALL_UINT_ID, new List<IArgumentFactory>() { uintFunctionArgumentType, byteArgumentType } ),
                (CommandType.CALL_BYTE_ID, new List<IArgumentFactory>() { byteFunctionArgumentType, byteArgumentType } ),
                (CommandType.MASK_VEIP, new List<IArgumentFactory>() { } ),
                (CommandType.PUSH_R32, new List<IArgumentFactory>() { } ),
                (CommandType.POP_R32, new List<IArgumentFactory>() { } ),
                (CommandType.PUSH_INT32, new List<IArgumentFactory>() { uintArgumentType } ),
                (CommandType.PUSH_STR_BYTE, new List<IArgumentFactory>() { byteStrArgumentType } ),
                (CommandType.PUSH_STR_SHORT, new List<IArgumentFactory>() { ushortStrArgumentType } ),
                (CommandType.NONE, new List<IArgumentFactory>() {  } ),
                (CommandType.PUSH_STR_INT, new List<IArgumentFactory>() { uintStrArgumentType } ),
                (CommandType.PUSH_UINT32, new List<IArgumentFactory>() { uintArgumentType } ),
                (CommandType.POP, new List<IArgumentFactory>() {} ),
                (CommandType.PUSH_0, new List<IArgumentFactory>() {} ),
                (CommandType.UNKNOWN_1, new List<IArgumentFactory>() {} ),
                (CommandType.PUSH_0x, new List<IArgumentFactory>() { byteArgumentType } ),
                (CommandType.PUSH_SP, new List<IArgumentFactory>() { } ),
                (CommandType.NEG, new List<IArgumentFactory>() { } ),
                (CommandType.ADD, new List<IArgumentFactory>() { } ),
                (CommandType.SUB, new List<IArgumentFactory>() { } ),
                (CommandType.MUL, new List<IArgumentFactory>() { } ),
                (CommandType.DIV, new List<IArgumentFactory>() { } ),
                (CommandType.MOD, new List<IArgumentFactory>() { } ),
                (CommandType.AND, new List<IArgumentFactory>() { } ),
                (CommandType.OR, new List<IArgumentFactory>() { } ),
                (CommandType.XOR, new List<IArgumentFactory>() { } ),
                (CommandType.NOT, new List<IArgumentFactory>() { } ),
                (CommandType.BOOL1, new List<IArgumentFactory>() { } ),
                (CommandType.BOOL2, new List<IArgumentFactory>() { } ),
                (CommandType.BOOL3, new List<IArgumentFactory>() { } ),
                (CommandType.BOOL4, new List<IArgumentFactory>() { } ),
                (CommandType.ISL, new List<IArgumentFactory>() { } ),
                (CommandType.ISLE, new List<IArgumentFactory>() { } ),
                (CommandType.ISNLE, new List<IArgumentFactory>() { } ),
                (CommandType.ISNL, new List<IArgumentFactory>() { } ),
                (CommandType.ISEQ, new List<IArgumentFactory>() { } ),
                (CommandType.ISNEQ, new List<IArgumentFactory>() { } ),
                (CommandType.SHL, new List<IArgumentFactory>() { } ),
                (CommandType.SAR, new List<IArgumentFactory>() { } ),
                (CommandType.INC, new List<IArgumentFactory>() { } ),
                (CommandType.DEC, new List<IArgumentFactory>() { } ),
                (CommandType.ADD_REG, new List<IArgumentFactory>() { } ),
                (CommandType.DEBUG, new List<IArgumentFactory>() { } ),
                (CommandType.CALL_UINT_NO_PARAM, new List<IArgumentFactory>() { uintFunctionArgumentType } ),
                (CommandType.ADD_2, new List<IArgumentFactory>() { } ),
                (CommandType.FPCOPY, new List<IArgumentFactory>() { } ),
                (CommandType.FPGET, new List<IArgumentFactory>() { } ),
                (CommandType.INITSTACK, new List<IArgumentFactory>() { uintArgumentType } ),
                (CommandType.UNKNOWN_2, new List<IArgumentFactory>() { } ),
                (CommandType.RET, new List<IArgumentFactory>() { byteArgumentType } )
            };
        }
        public Dictionary<string, VarItem> Vars { get; set; }
        public Dictionary<string, FunctionItem> Functions { get; set; }
        public Dictionary<string, LabelItem> Labels { get; set; }
        public Dictionary<string,ContentItem> ContentItems { get; set; }
        private Dictionary<uint, StringsItem> ContentStringsOffsets { get; set; } = new Dictionary<uint, StringsItem>();
        public List<CommandItem> Commands { get; set; }
        private uint Magic { get; set; }
        public int VMCodeLength { get; set; }
        public List<LineItem> Strings { get; set; }
        public MSEScript(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                ReadTitles(reader);
                ReadContents(reader);
                ReadCode(reader);
                ReadStrings(reader);
            }
        }
        public MSEScript(string filename) : this(File.ReadAllBytes(filename))
        {
        }
        private void ReadVars(BinaryReader reader)
        {
            var count = reader.ReadUInt32();
            var vars = new Dictionary<string, VarItem>();
            for (int i = 0; i < count; i++)
            {
                var length = reader.ReadUInt32();
                if ((length >> 24) != 0x80)
                {
                    throw new Exception();
                }
                length = length & 0x7fFFFFFF;
                var bytes = reader.ReadBytes((int)length);
                var name = Encoding.Unicode.GetString(bytes).TrimEnd('\0');
                var parameters = new List<uint>();
                while (true)
                {
                    var flag = reader.ReadUInt32();
                    parameters.Add(flag);
                    if(flag == 0)
                    {
                        break;
                    }
                    var value = reader.ReadUInt32();
                    parameters.Add(value);
                }
                for (int j = 0; j < 4; j++)
                {
                    var value = reader.ReadUInt32();
                    parameters.Add(value);
                }
                var varItem = new VarItem { Name=name,Parameters=parameters };
                vars.Add(name, varItem);
            }
            Vars = vars;
        }
        private void ReadFunctions(BinaryReader reader)
        {
            Magic = reader.ReadUInt32();
            var count = reader.ReadUInt32();
            var functions = new Dictionary<string, FunctionItem>();
            for (int i = 0; i < count; i++)
            {
                var length = reader.ReadUInt32();
                if ((length >> 24) != 0x80)
                {
                    throw new Exception();
                }
                length = length & 0x7FFFFFFF;

                var function = new FunctionItem();
                function.Name = Encoding.Unicode.GetString(reader.ReadBytes((int)length)).TrimEnd('\0');
                function.Id = reader.ReadUInt32();
                function.Reserved0 = reader.ReadUInt32();
                function.VMCodeOffset = reader.ReadUInt32();
                functions.Add(function.Name, function);
            }
            Functions = functions;
        }
        private void ReadLabels(BinaryReader reader)
        {
            var count = reader.ReadUInt32();
            var labels = new Dictionary<string, LabelItem>();
            for (int i = 0; i < count; i++)
            {
                var length = reader.ReadUInt32();
                if ((length >> 24) != 0x80)
                {
                    throw new Exception();
                }

                length = length & 0x7FFFFFFF;
                var label = new LabelItem();
                label.Name = Encoding.Unicode.GetString(reader.ReadBytes((int)length)).TrimEnd('\0');
                label.VMCodeOffset = reader.ReadUInt32();
                labels.Add(label.Name, label);
            }
            Labels = labels;
        }
        private void ReadTitles(BinaryReader reader)
        {
            ReadVars(reader);
            ReadFunctions(reader);
            ReadLabels(reader);
        }
        private string ReadString(BinaryReader reader)
        {
            var start = reader.BaseStream.Position;
            while (reader.ReadUInt16() != 0)
            {
            }
            var end = reader.BaseStream.Position;
            reader.BaseStream.Position = start;
            var length = (int)(end - start);
            var bytes = reader.ReadBytes(length);
            var text = Encoding.Unicode.GetString(bytes).TrimEnd('\0');
            if (text == "\a\f1")
            {
                text = "[NAME]"+ReadString(reader);
            }
            return text;
        }
        private void ReadContents(BinaryReader reader)
        {
            bool is_continue = true;
            var sectionSize = reader.ReadUInt32();
            var contentOffset = reader.BaseStream.Position;
            ContentItems = new Dictionary<string, ContentItem>();
            ContentItem contentItem = null;
            while (is_continue)
            {
                var start = reader.BaseStream.Position;
                var offset = (uint)(start - contentOffset);
                var text = ReadString(reader);
                var stringItem = new StringsItem(offset, text);
                ContentStringsOffsets.Add(offset, stringItem);
                if (text.StartsWith("■"))
                {
                    contentItem = new ContentItem
                    {
                        Title = stringItem,
                        Offset = (uint)start,
                        Texts = new List<StringsItem>()
                    };
                    ContentItems.Add(text.Escape(), contentItem);
                }
                else
                {
                    contentItem.Texts.Add(stringItem);
                }
                if (text == "■シナリオ終了\n")
                {
                    break;
                }
            };
        }
        private void ReadCode(BinaryReader reader)
        {
            VMCodeLength = reader.ReadInt32();
            var end = reader.BaseStream.Position + VMCodeLength;

            var commands = new List<CommandItem>();
            while (reader.BaseStream.Position != end)
            {
                var offset = reader.BaseStream.Position;
                var code = reader.ReadByte();
                var (commandType,argumentFactories) = CommandsDict[code];
                var arguments = argumentFactories.Select(
                    argumentFactory =>
                    {
                        var argument = argumentFactory.Create();
                        argument.Read(reader, ContentStringsOffsets);
                        return argument;
                        }
                    ).ToList();
                var command = new CommandItem((int)offset, commandType, arguments);
                commands.Add(command);
            }
            Commands = commands;
            foreach(var str in ContentStringsOffsets)
            {
                if(str.Value.Arguments.Count == 0 && str.Value.Text != "[EMPTY]")
                {
                    throw new Exception("Some content string doesn't used. May be error in code");
                }
            }
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
                var line = Encoding.Unicode.GetString(bytes);
                var text = new LineItem(line);

                Strings.Add(text);
            }
        }
        private void WriteVars(BinaryWriter writer)
        {
            writer.Write(Vars.Count);
            foreach(var pair in Vars)
            {
                var bytes = Encoding.Unicode.GetBytes(pair.Key + '\0');
                writer.Write((uint)(bytes.Length | 0x80000000));
                writer.Write(bytes);
                foreach(var parameter in pair.Value.Parameters)
                {
                    writer.Write(parameter);
                }
            }
        }
        private void WriteFunctions(BinaryWriter writer)
        {
            writer.Write(Magic);
            writer.Write(Functions.Count);
            foreach (var pair in Functions)
            {
                var bytes = Encoding.Unicode.GetBytes(pair.Key + '\0');
                writer.Write((uint)(bytes.Length | 0x80000000));
                writer.Write(bytes);
                writer.Write(pair.Value.Id);
                writer.Write(pair.Value.Reserved0);
                writer.Write(pair.Value.VMCodeOffset);
            }
        }
        private void WriteLabels(BinaryWriter writer)
        {
            writer.Write(Labels.Count);
            foreach (var pair in Labels)
            {
                var bytes = Encoding.Unicode.GetBytes(pair.Key + '\0');
                writer.Write((uint)(bytes.Length | 0x80000000));
                writer.Write(bytes);
                writer.Write(pair.Value.VMCodeOffset);
            }
        }
        private void WriteTitles(BinaryWriter writer)
        {
            WriteVars(writer);
            WriteFunctions(writer);
            WriteLabels(writer);
        }
        public void WriteContents(BinaryWriter writer)
        {
            var startPos = writer.BaseStream.Position;

            foreach (var contentItem in ContentItems.Values)
            {
                foreach (var stringItem in contentItem.Texts.Prepend(contentItem.Title))
                {
                    var offset = (uint)(writer.BaseStream.Position - startPos);
                    
                    foreach (var arg in stringItem.Arguments)
                    {
                        arg.Value = offset;
                    }
                    stringItem.Offset = offset;
                    var line = stringItem.Dump().Replace("[NAME]", "\a\f1\0") + "\0";
                    var bytes = Encoding.Unicode.GetBytes(line);
                    writer.Write(bytes);
                }
            }
            var bytesLength = writer.BaseStream.Position - startPos;
            writer.BaseStream.Position = startPos;
            writer.Write((int)bytesLength);
            foreach (var contentItem in ContentItems.Values)
            {
                foreach (var stringItem in contentItem.Texts.Prepend(contentItem.Title))
                {
                    var line = stringItem.Dump().Replace("[NAME]", "\a\f1\0") + "\0";
                    var bytes = Encoding.Unicode.GetBytes(line);
                    writer.Write(bytes);
                }
            }
        }
        public void WriteCode(BinaryWriter writer)
        {
            writer.Write(VMCodeLength);
            foreach(var command in Commands)
            {
                var code = (byte)command.Type;
                writer.Write(code);
                foreach(var arg in command.Args)
                {
                    arg.Write(writer);
                }
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
                WriteTitles(writer);
                WriteContents(writer);
                WriteCode(writer);
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
