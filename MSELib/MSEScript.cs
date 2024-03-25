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
        public static IReadOnlyList<(CommandType, IReadOnlyList<ArgumentType>)> CommandsDict = new List<(CommandType, IReadOnlyList<ArgumentType>)>
        {
            (CommandType.JMP, new List<ArgumentType>() {ArgumentType.COMMAND_PTR} ),
            (CommandType.JNZ, new List<ArgumentType>() {ArgumentType.COMMAND_PTR } ),
            (CommandType.JZ, new List<ArgumentType>() {ArgumentType.COMMAND_PTR } ),
            (CommandType.CALL_UINT_ID, new List<ArgumentType>() {ArgumentType.FUNCTION_INT_ID,ArgumentType.BYTE} ),
            (CommandType.CALL_BYTE_ID, new List<ArgumentType>() {ArgumentType.FUNCTION_BYTE_ID,ArgumentType.BYTE} ),
            (CommandType.MASK_VEIP, new List<ArgumentType>() { } ),
            (CommandType.PUSH_R32, new List<ArgumentType>() { } ),
            (CommandType.POP_R32, new List<ArgumentType>() { } ),
            (CommandType.PUSH_INT32, new List<ArgumentType>() { ArgumentType.INT} ),
            (CommandType.PUSH_STR_BYTE, new List<ArgumentType>() { ArgumentType.STR_BYTE_ID} ),
            (CommandType.PUSH_STR_SHORT, new List<ArgumentType>() { ArgumentType.STR_SHORT_ID} ),
            (CommandType.NONE, new List<ArgumentType>() {  } ),
            (CommandType.PUSH_STR_INT, new List<ArgumentType>() { ArgumentType.STR_INT_ID} ),
            (CommandType.PUSH_UINT32, new List<ArgumentType>() { ArgumentType.INT} ),
            (CommandType.POP, new List<ArgumentType>() {} ),
            (CommandType.PUSH_0, new List<ArgumentType>() {} ),
            (CommandType.UNKNOWN_1, new List<ArgumentType>() {} ),
            (CommandType.PUSH_0x, new List<ArgumentType>() { ArgumentType.BYTE} ),
            (CommandType.PUSH_SP, new List<ArgumentType>() { } ),
            (CommandType.NEG, new List<ArgumentType>() { } ),
            (CommandType.ADD, new List<ArgumentType>() { } ),
            (CommandType.SUB, new List<ArgumentType>() { } ),
            (CommandType.MUL, new List<ArgumentType>() { } ),
            (CommandType.DIV, new List<ArgumentType>() { } ),
            (CommandType.MOD, new List<ArgumentType>() { } ),
            (CommandType.AND, new List<ArgumentType>() { } ),
            (CommandType.OR, new List<ArgumentType>() { } ),
            (CommandType.XOR, new List<ArgumentType>() { } ),
            (CommandType.NOT, new List<ArgumentType>() { } ),
            (CommandType.BOOL1, new List<ArgumentType>() { } ),
            (CommandType.BOOL2, new List<ArgumentType>() { } ),
            (CommandType.BOOL3, new List<ArgumentType>() { } ),
            (CommandType.BOOL4, new List<ArgumentType>() { } ),
            (CommandType.ISL, new List<ArgumentType>() { } ),
            (CommandType.ISLE, new List<ArgumentType>() { } ),
            (CommandType.ISNLE, new List<ArgumentType>() { } ),
            (CommandType.ISNL, new List<ArgumentType>() { } ),
            (CommandType.ISEQ, new List<ArgumentType>() { } ),
            (CommandType.ISNEQ, new List<ArgumentType>() { } ),
            (CommandType.SHL, new List<ArgumentType>() { } ),
            (CommandType.SAR, new List<ArgumentType>() { } ),
            (CommandType.INC, new List<ArgumentType>() { } ),
            (CommandType.DEC, new List<ArgumentType>() { } ),
            (CommandType.ADD_REG, new List<ArgumentType>() { } ),
            (CommandType.DEBUG, new List<ArgumentType>() { } ),
            (CommandType.CALL_UINT_NO_PARAM, new List<ArgumentType>() { ArgumentType.FUNCTION_INT_ID } ),
            (CommandType.ADD_2, new List<ArgumentType>() { } ),
            (CommandType.FPCOPY, new List<ArgumentType>() { } ),
            (CommandType.FPGET, new List<ArgumentType>() { } ),
            (CommandType.INITSTACK, new List<ArgumentType>() { ArgumentType.INT } ),
            (CommandType.UNKNOWN_2, new List<ArgumentType>() { } ),
            (CommandType.RET, new List<ArgumentType>() { ArgumentType.BYTE } )
        };
        public Dictionary<string, VarItem> Vars { get; set; }
        public Dictionary<string, FunctionItem> Functions { get; set; }
        public Dictionary<string, LabelItem> Labels { get; set; }
        public Dictionary<string, ContentItem> ContentItems { get; set; }
        private Dictionary<uint, StringsItem> ContentStringsOffsets { get; set; } = new Dictionary<uint, StringsItem>();
        public List<CommandItem> Commands { get; set; }
        private uint Magic { get; set; }
        public int VMCodeLength { get; set; }
        public int ContentsLength { get; set; }
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
                    if (flag == 0)
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
                var varItem = new VarItem { Name = name, Parameters = parameters };
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
                function.VMCodeOffset = reader.ReadInt32();
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
                label.VMCodeOffset = reader.ReadInt32();
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
                text = "[NAME]" + ReadString(reader);
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
                var offset = (int)(start - contentOffset);
                var text = ReadString(reader);
                var stringItem = new StringsItem(offset, text);
                ContentStringsOffsets.Add((uint)offset, stringItem);
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
        private void AddStringArgument(uint offset, ArgumentItem argument)
        {
            if (!ContentStringsOffsets.TryGetValue(offset, out var stringsItem))
            {
                throw new Exception("Doesn't found string for argument");
            }
            stringsItem.Arguments.Add(argument);
        }
        private ArgumentItem ParseArgument(BinaryReader reader, ArgumentType argumentType,List<ArgumentItem> commandPtrs)
        {
            var argument = new ArgumentItem((int)reader.BaseStream.Position, argumentType, 0);
            switch (argumentType)
            {
                case ArgumentType.BYTE:
                    argument.Value = reader.ReadByte();
                    break;
                case ArgumentType.SHORT:
                    argument.Value = reader.ReadInt16();
                    break;
                case ArgumentType.INT:
                    argument.Value = reader.ReadInt32();
                    break;
                case ArgumentType.COMMAND_PTR:
                    argument.Value = reader.ReadInt32();
                    commandPtrs.Add(argument);
                    break;
                case ArgumentType.STR_BYTE_ID:
                    {
                        var offset = reader.ReadByte();
                        argument.Value = offset;
                        AddStringArgument(offset, argument);
                    }
                    break;
                case ArgumentType.STR_SHORT_ID:
                    {
                        var offset = reader.ReadUInt16();
                        argument.Value = offset;
                        AddStringArgument(offset, argument);
                    }
                    break;
                case ArgumentType.STR_INT_ID:
                    {
                        var offset = reader.ReadUInt32();
                        argument.Value = (int)offset;
                        AddStringArgument(offset, argument);
                    }
                    break;
                case ArgumentType.FUNCTION_BYTE_ID:
                    argument.Value = reader.ReadByte();
                    break;
                case ArgumentType.FUNCTION_INT_ID:
                    argument.Value = reader.ReadInt32();
                    break;
                default:
                    throw new NotImplementedException();
            }
            return argument;
        }
        private void ReadCode(BinaryReader reader)
        {
            VMCodeLength = reader.ReadInt32();
            var codeOffset = reader.BaseStream.Position;
            var end = reader.BaseStream.Position + VMCodeLength;

            var commands = new List<CommandItem>();
            var commandsTable = new Dictionary<int, CommandItem>();
            var commandPtrs = new List<ArgumentItem>();
            while (reader.BaseStream.Position != end)
            {
                var offset = (int)(reader.BaseStream.Position-codeOffset);
                var code = reader.ReadByte();
                var (commandType, argumentsTypes) = CommandsDict[code];
                var arguments = argumentsTypes.Select(
                    argumentType => ParseArgument(reader, argumentType,commandPtrs)).ToList();
                var command = new CommandItem(offset, commandType, arguments);
                foreach(var arg in command.Args)
                {
                    arg.Command = command;
                }
                commandsTable.Add(offset, command);
                commands.Add(command);
            }
            int index = 0;
            foreach (var arg in commandPtrs)
            {
                //IDK why just 3 bytes
                var offset = arg.Value & 0xffffff;// arg.Command.Offset + arg.Value- codeOffset;
                if (!commandsTable.TryGetValue(offset, out var command))
                {
                    throw new Exception("Command ptr incorrect");
                }
                arg.CommandPtr = command;
                index++;
            }
            foreach(var function in Functions)
            {
                if(!commandsTable.TryGetValue(function.Value.VMCodeOffset,out var command))
                {
                    throw new Exception("Function command ptr incorrect");
                }
                function.Value.Command = command;
            }
            foreach(var label in Labels)
            {
                if(!commandsTable.TryGetValue(label.Value.VMCodeOffset,out var command))
                {
                    throw new Exception("Label command ptr incorrect");
                }
                label.Value.Command = command;
            }
            Commands = commands;
            foreach (var str in ContentStringsOffsets)
            {
                if (str.Value.Arguments.Count == 0 && str.Value.Text != "[EMPTY]")
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
            foreach (var pair in Vars)
            {
                var bytes = Encoding.Unicode.GetBytes(pair.Key + '\0');
                writer.Write((uint)(bytes.Length | 0x80000000));
                writer.Write(bytes);
                foreach (var parameter in pair.Value.Parameters)
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
                writer.Write(pair.Value.Command.Offset);
                //writer.Write(pair.Value.VMCodeOffset);
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
                writer.Write(pair.Value.Command.Offset);
                //writer.Write(pair.Value.VMCodeOffset);
            }
        }
        private void WriteTitles(BinaryWriter writer)
        {
            WriteVars(writer);
            WriteFunctions(writer);
            WriteLabels(writer);
        }
        private int CalcCodeOffsets()
        {
            var offset = 0;
            foreach (var command in Commands)
            {
                command.Offset = offset;
                offset += sizeof(byte);
                foreach (var arg in command.Args)
                {
                    switch (arg.Type)
                    {
                        case ArgumentType.BYTE:
                        case ArgumentType.FUNCTION_BYTE_ID:
                        case ArgumentType.STR_BYTE_ID:
                            offset += sizeof(byte);
                            break;
                        case ArgumentType.SHORT:
                        case ArgumentType.STR_SHORT_ID:
                            offset += sizeof(ushort);
                            break;
                        case ArgumentType.INT:
                        case ArgumentType.FUNCTION_INT_ID:
                        case ArgumentType.STR_INT_ID:
                            offset += sizeof(int);
                            break;
                        case ArgumentType.COMMAND_PTR:
                            offset += sizeof(int);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return offset;
        }
        private int CalcContentsOffsets()
        {
            var offset = 0;
            foreach (var contentItem in ContentItems.Values)
            {
                foreach (var stringItem in contentItem.Texts.Prepend(contentItem.Title))
                {
                    foreach (var arg in stringItem.Arguments)
                    {
                        if (arg.Type is ArgumentType.STR_BYTE_ID)
                        {
                            if (offset > 0xFF)
                            {
                                if (offset > 0xFFFF)
                                {
                                    arg.Type = ArgumentType.STR_INT_ID;
                                    arg.Command.Type = CommandType.PUSH_STR_INT;
                                }
                                else
                                {
                                    arg.Type = ArgumentType.STR_SHORT_ID;
                                    arg.Command.Type = CommandType.PUSH_STR_SHORT;
                                }
                            }
                            arg.Value = offset;
                        }
                        else if (arg.Type is ArgumentType.STR_SHORT_ID)
                        {
                            if (offset > 0xFFFF)
                            {
                                arg.Type = ArgumentType.STR_INT_ID;
                                arg.Command.Type = CommandType.PUSH_STR_INT;
                            }
                            arg.Value = offset;
                        }
                        else if (arg.Type is ArgumentType.STR_INT_ID)
                        {
                            arg.Value = offset;
                        }
                    }
                    stringItem.Offset = offset;
                    var line = stringItem.Dump().Replace("[NAME]", "\a\f1\0") + "\0";
                    var bytes = Encoding.Unicode.GetBytes(line);
                    offset += bytes.Length;
                }
            }
            return offset;
        }
        public void WriteContents(BinaryWriter writer)
        {
            writer.Write(ContentsLength);
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
            foreach (var command in Commands)
            {
                var code = (byte)command.Type;
                writer.Write(code);
                foreach (var arg in command.Args)
                {
                    switch (arg.Type)
                    {
                        case ArgumentType.BYTE:
                        case ArgumentType.FUNCTION_BYTE_ID:
                        case ArgumentType.STR_BYTE_ID:
                            writer.Write((byte)arg.Value);
                            break;
                        case ArgumentType.SHORT:
                        case ArgumentType.STR_SHORT_ID:
                            writer.Write((ushort)arg.Value);
                            break;
                        case ArgumentType.INT:
                        case ArgumentType.FUNCTION_INT_ID:
                        case ArgumentType.STR_INT_ID:
                            writer.Write(arg.Value);
                            break;
                        case ArgumentType.COMMAND_PTR:
                            arg.Value = (arg.CommandPtr.Offset & 0xffffff) | (arg.Value & 0x0f000000);
                            writer.Write(arg.Value);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
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
                ContentsLength = CalcContentsOffsets();
                VMCodeLength = CalcCodeOffsets();
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
