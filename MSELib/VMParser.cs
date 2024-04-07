using MSELib.classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MSELib
{
    internal class VMParser
    {
        public uint Magic { get; set; }
        public uint VMDataOffset { get; set; }
        public uint VMDataLength { get; set; }
        public uint VMCodeOffset { get; set; }
        public uint VMCodeLength { get; set; }
        public List<VarItem> Vars { get; private set; }
        public List<FunctionItem> Functions { get; private set; }
        public Dictionary<string, FunctionItem> FunctionsByName { get; private set; }
        public Dictionary<uint, FunctionItem> FunctionsById { get; private set; }
        public List<LabelItem> Labels { get; private set; }
        public Dictionary<string, LabelItem> LabelsByName { get; private set; }
        public List<StringItem> DataStrings { get; set; }
        public Dictionary<uint, StringItem> DataStringsByOffset { get; private set; }
        public List<Chapter> Chapters { get; private set; }
        public List<BaseCommand> Commands { get; private set; }
        public Dictionary<uint,BaseCommand> CommandsTable { get; private set; }
        public List<LineItem> Strings { get; private set; }
        public VMParser(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                ReadTitles(reader);
                ReadVMData(reader);
                var chapterNames = new List<string>();
                var chapterIndex = new List<uint>();
                var moji = ReadCode(reader, chapterNames, chapterIndex);
                ReadStrings(reader);
                CreateChapters(moji, chapterNames, chapterIndex);
                DataStrings = DataStrings.OrderBy(x => x.Offset).ToList();
            }
        }
        public VMParser(string filename) : this(File.ReadAllBytes(filename))
        {

        }
        private void CreateChapters(List<ChapterStringConfig> moji, List<string> chapterNames, List<uint> chapterIndex)
        {
            var chapterRegion = chapterIndex.Skip(1).Append((uint)moji.Count).ToList();
            var chapters = new List<Chapter>();
            for (int i = 0; i < chapterIndex.Count; i++)
            {
                var index = chapterIndex[i];
                string title = chapterNames[i];
                var endIndex = (int)chapterRegion[i];
                var lines = new List<ChapterString>();
                for (var j = (int)index; j < endIndex; j++)
                {
                    var name = moji[j].Name;
                    var text = Strings[(int)moji[j].Index];
                    lines.Add(new ChapterString(name, text));
                }
                chapters.Add(new Chapter(title, lines,(int)index,endIndex));
            }
            Chapters = chapters;
        }
        private void ReadVars(BinaryReader reader)
        {
            var count = reader.ReadUInt32();
            Vars = new List<VarItem>();
            for (int i = 0; i < count; i++)
            {
                var length = reader.ReadUInt32();
                if ((length >> 24) != 0x80)
                {
                    throw new Exception();
                }
                length &= 0x7fFFFFFF;
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
                Vars.Add(varItem);
            }
        }
        private void ReadFunctions(BinaryReader reader)
        {
            Magic = reader.ReadUInt32();
            var count = reader.ReadUInt32();
            FunctionsByName = new Dictionary<string, FunctionItem>();
            FunctionsById = new Dictionary<uint, FunctionItem>();
            Functions = new List<FunctionItem>();
            for (int i = 0; i < count; i++)
            {
                var length = reader.ReadUInt32();
                if ((length >> 24) != 0x80)
                {
                    throw new Exception();
                }
                length &= 0x7FFFFFFF;

                var function = new FunctionItem
                {
                    Name = Encoding.Unicode.GetString(reader.ReadBytes((int)length)).TrimEnd('\0'),
                    Id = reader.ReadUInt32(),
                    Reserved0 = reader.ReadUInt32(),
                    VMCodeOffset = reader.ReadUInt32()
                };
                FunctionsByName.Add(function.Name, function);
                FunctionsById.Add(function.Id, function);
                Functions.Add(function);
            }
        }
        private void ReadLabels(BinaryReader reader)
        {
            var count = reader.ReadUInt32();
            LabelsByName = new Dictionary<string, LabelItem>();
            Labels = new List<LabelItem>();
            for (int i = 0; i < count; i++)
            {
                var length = reader.ReadUInt32();
                if ((length >> 24) != 0x80)
                {
                    throw new Exception();
                }

                length &= 0x7FFFFFFF;
                var label = new LabelItem
                {
                    Name = Encoding.Unicode.GetString(reader.ReadBytes((int)length)).TrimEnd('\0'),
                    VMCodeOffset = reader.ReadUInt32()
                };
                LabelsByName.Add(label.Name, label);
                Labels.Add(label);
            }
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
        private void ReadVMData(BinaryReader reader)
        {
            VMDataLength = reader.ReadUInt32();
            VMDataOffset = (uint)reader.BaseStream.Position;
            reader.BaseStream.Position += VMDataLength;
            //DataStrings = new List<StringItem>();
            //string text = null;
            //while (text != "■シナリオ終了\n")
            //{
            //    var start = (uint)reader.BaseStream.Position;
            //    var offset = start - VMDataOffset;
            //    text = ReadString(reader);
            //    var stringItem = new StringItem(text, offset);
            //    DataStrings.Add(stringItem);
            //};
            //if(reader.BaseStream.Position != VMDataOffset + VMDataLength)
            //{
            //    reader.BaseStream.Position = VMDataOffset + VMDataLength;
            //}
        }
        static string ExtractValue(string input, string pattern)
        {
            Match match = Regex.Match(input, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
        private void ParseJmpTable(BinaryReader reader, List<uint> jmpTable, long end, ref uint pParma, ref StringItem pLastString, Stack<uint> vmStack, List<IJmpCommand> jumps)
        {
            var MALIE_END = FunctionsByName["MALIE_END"].Id;
            while (reader.BaseStream.Position != end)
            {
                var offset = (uint)(reader.BaseStream.Position - VMCodeOffset);
                var command = ParseCommand(reader, ref pParma, ref pLastString, vmStack, jumps);
                CommandsTable.Add(offset, command);
                Commands.Add(command);
                command.Offset = offset;
                if (command.Type == CommandType.CALL_UINT_NO_PARAM)
                {
                    if (pParma == MALIE_END)
                    {
                        break;
                    }
                }
                else if (command.Type == CommandType.JMP)
                {
                    jmpTable.Add(pParma);
                }
            }
        }
        private StringItem ReadDataString(BinaryReader reader, uint offset)
        {
            var localOffset = offset - VMDataOffset;
            if (DataStringsByOffset.TryGetValue(localOffset, out var value))
            {
                return value;
            }
            var save_offset = reader.BaseStream.Position;
            reader.BaseStream.Position = offset;
            var str = ReadString(reader);
            var stringItem = new StringItem(str, localOffset);
            reader.BaseStream.Position = save_offset;
            DataStringsByOffset.Add(localOffset, stringItem);
            DataStrings.Add(stringItem);
            return stringItem;
        }
        private BaseCommand ParseCommand(BinaryReader reader, ref uint pParma, ref StringItem pLastString, Stack<uint> vmStack, List<IJmpCommand> jumps)
        {
            var code = reader.ReadByte();
            var commandType = (CommandType)code;

            switch (commandType)
            {
                case CommandType.JMP:
                case CommandType.JNZ:
                case CommandType.JZ:
                    {
                        pParma = reader.ReadUInt32();
                        var command = new JmpCommand(commandType, pParma);
                        jumps.Add(command);
                        return command;
                    }
                case CommandType.CALL_UINT_ID:
                    {
                        pParma = reader.ReadUInt32();
                        var arg = reader.ReadByte();
                        if (!FunctionsById.TryGetValue(pParma, out var function))
                        {
                            throw new Exception($"Function with Id {pParma} not found");
                        }
                        return new CallCommand(commandType, function, arg);
                    }
                case CommandType.CALL_BYTE_ID:
                    {
                        pParma = reader.ReadByte();
                        var arg = reader.ReadByte();
                        if (!FunctionsById.TryGetValue(pParma, out var function))
                        {
                            throw new Exception($"Function with Id {pParma} not found");
                        }
                        return new CallCommand(commandType, function, arg);
                    }
                case CommandType.MASK_VEIP:
                    return new NoArgumentCommand(commandType);
                case CommandType.PUSH_R32:
                    return new NoArgumentCommand(commandType);
                case CommandType.POP_R32:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.PUSH_INT32:
                case CommandType.PUSH_UINT32:
                    pParma = reader.ReadUInt32();
                    vmStack.Push(pParma | 0x80000000);
                    return new UIntArgumentCommand(commandType, pParma);
                case CommandType.PUSH_STR_BYTE:
                    {
                        pParma = reader.ReadByte();
                        var offset = pParma + VMDataOffset;
                        pLastString = ReadDataString(reader, offset);
                        vmStack.Push(offset);
                        return new PushStringCommand(commandType, pLastString);
                    }
                case CommandType.PUSH_STR_SHORT:
                    {
                        pParma = reader.ReadUInt16();
                        var offset = pParma + VMDataOffset;
                        pLastString = ReadDataString(reader, offset);
                        vmStack.Push(offset);
                        return new PushStringCommand(commandType, pLastString);
                    }
                case CommandType.NONE:
                    return new NoArgumentCommand(commandType);
                case CommandType.PUSH_STR_INT:
                    {
                        pParma = reader.ReadUInt32();
                        var offset = pParma + VMDataOffset;
                        pLastString = ReadDataString(reader, offset);
                        vmStack.Push(offset);
                        return new PushStringCommand(commandType, pLastString);
                    }
                case CommandType.POP:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.PUSH_0:
                    vmStack.Push(0 | 0x80000000);
                    return new NoArgumentCommand(commandType);
                case CommandType.UNKNOWN_1:
                    return new NoArgumentCommand(commandType);
                case CommandType.PUSH_0x:
                    {
                        var arg = reader.ReadByte();
                        pParma = arg;
                        vmStack.Push(pParma | 0x80000000);
                        return new ByteArgumentCommand(commandType, arg);
                    }
                case CommandType.PUSH_SP:
                    return new NoArgumentCommand(commandType);
                case CommandType.NEG:
                    return new NoArgumentCommand(commandType);
                case CommandType.ADD:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.SUB:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.MUL:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.DIV:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.MOD:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.AND:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.OR:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.XOR:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.BOOL1:
                    return new NoArgumentCommand(commandType);
                case CommandType.BOOL2:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.BOOL3:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.BOOL4:
                    return new NoArgumentCommand(commandType);
                case CommandType.ISL:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.ISLE:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.ISNLE:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.ISNL:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.ISEQ:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.ISNEQ:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.SHL:
                    //vmStack.Pop();
                    //vmStack.Peek();
                    return new NoArgumentCommand(commandType);
                case CommandType.SAR:
                    vmStack.Pop();
                    return new NoArgumentCommand(commandType);
                case CommandType.INC:
                    return new NoArgumentCommand(commandType);
                case CommandType.DEC:
                    return new NoArgumentCommand(commandType);
                case CommandType.ADD_REG:
                    return new NoArgumentCommand(commandType);
                case CommandType.DEBUG:
                    return new NoArgumentCommand(commandType);
                case CommandType.CALL_UINT_NO_PARAM:
                    {
                        pParma = reader.ReadUInt32();
                        vmStack.Push(pParma | 0x80000000);
                        if (!FunctionsById.TryGetValue(pParma, out var function))
                        {
                            throw new Exception($"Function with Id {pParma} not found");
                        }
                        return new CallCommand(commandType, function);
                    }
                case CommandType.ADD_2:
                    return new NoArgumentCommand(commandType);
                case CommandType.FPCOPY:
                    return new NoArgumentCommand(commandType);
                case CommandType.FPGET:
                    return new NoArgumentCommand(commandType);
                case CommandType.INITSTACK:
                    pParma = reader.ReadUInt32();
                    return new UIntArgumentCommand(commandType, pParma);
                case CommandType.Unknown2:
                    return new NoArgumentCommand(commandType);
                case CommandType.RET:
                    {
                        var temp = reader.ReadByte();
                        pParma = temp;
                        return new ByteArgumentCommand(commandType, temp);
                    }
                default:
                    return null;
            }

        }
        private List<ChapterStringConfig> ReadCode(BinaryReader reader, List<string> chapterName, List<uint> chapterIndex)
        {
            VMCodeLength = reader.ReadUInt32();
            DataStrings = new List<StringItem>();
            DataStringsByOffset = new Dictionary<uint, StringItem>();
            VMCodeOffset = (uint)reader.BaseStream.Position;
            var end = reader.BaseStream.Position + VMCodeLength;

            var v = new List<ChapterStringConfig>();
            var scenarioOffset = FunctionsByName["maliescenario"].VMCodeOffset;

            var _ms_message = FunctionsByName["_ms_message"].Id;
            var MALIE_NAME = FunctionsByName["MALIE_NAME"].Id;
            var MALIE_LABLE = FunctionsByName["MALIE_LABLE"].Id;
            var tag = FunctionsByName["tag"].Id;
            var FrameLayer_SendMessage = FunctionsByName["FrameLayer_SendMessage"].Id;
            var System_GetResult = FunctionsByName["System_GetResult"].Id;

            ChapterStringConfig moji = new ChapterStringConfig();
            var jmpTable = new List<uint>();
            var selectTable = new List<uint>();
            var jmpIteratorIndex = -1;

            Commands = new List<BaseCommand>();
            CommandsTable = new Dictionary<uint, BaseCommand>();

            var vmStack = new Stack<uint>();
            uint pParma = 0;
            StringItem pLastString = null;
            var jumps = new List<IJmpCommand>();
            while (reader.BaseStream.Position != end)
            {
                var offset = (uint)(reader.BaseStream.Position - VMCodeOffset);
                var command = ParseCommand(reader, ref pParma, ref pLastString, vmStack, jumps);
                command.Offset = offset;
                CommandsTable.Add(offset, command);
                Commands.Add(command);
                if (offset > scenarioOffset)
                {
                    if (jmpTable.Count != 0)
                    {
                        if (jmpIteratorIndex != jmpTable.Count && offset > jmpTable[jmpIteratorIndex])
                        {
                            jmpIteratorIndex++;
                            chapterIndex.Add((uint)v.Count);
                        }
                    }
                    if (command.Type == CommandType.CALL_UINT_NO_PARAM || command.Type == CommandType.CALL_BYTE_ID || command.Type == CommandType.CALL_UINT_ID)//vCall
                    {
                        if (pParma == tag)
                        {
                            var pos = pLastString.Text.IndexOf("<chapter");
                            if (pos != -1)
                            {
                                pLastString.Tag = StringTag.Chapter;
                                var Title = ExtractValue(pLastString.Text, @"<chapter name='(.*?)'>");

                                chapterName.Add(Title);

                                chapterIndex.Add((uint)v.Count);
                            }
                        }
                        else if (pParma == _ms_message)
                        {
                            vmStack.Pop();
                            moji.Index = vmStack.Peek() & ~0x80000000;
                            while (vmStack.Count != 0) vmStack.Pop();
                            vmStack.Push(0);
                            v.Add(moji);
                            moji = new ChapterStringConfig();
                            moji.Name = "";
                            selectTable.Clear();
                        }
                        else if (pParma == MALIE_NAME)
                        {
                            moji.Name = pLastString.Text;
                            pLastString.Tag = StringTag.Name;
                        }
                        else if (pParma == MALIE_LABLE && v.Count == 0)
                        {
                            if (pLastString.Text == "_index")
                            {
                                pLastString.Tag = StringTag.Label;
                                ParseJmpTable(reader, jmpTable, end, ref pParma, ref pLastString, vmStack, jumps);
                                jmpIteratorIndex = 0;
                                continue;
                            }
                        }
                        else if (pParma == System_GetResult)
                        {
                            foreach (var x in selectTable.Where(y=>y != 0x80000064))
                            {
                                var s = ReadDataString(reader, x);
                                s.Tag = StringTag.Select;
                            }
                        }
                        else if (pParma == FrameLayer_SendMessage && vmStack.Count > 4)
                        {
                            vmStack.Pop(); vmStack.Pop(); vmStack.Pop(); vmStack.Pop();
                            var loc = vmStack.Peek();
                            if (loc > 0)
                            {
                                selectTable.Add(loc);
                            }
                        }
                    }
                }
            }
            int index = 0;
            foreach (var jump in jumps)
            {
                //IDK why just 3 bytes
                var offset = jump.CommandOffset & 0xffffff;// arg.Command.Offset + arg.Value- codeOffset;
                if (!CommandsTable.TryGetValue(offset, out var command))
                {
                    throw new Exception("Command ptr incorrect");
                }
                jump.Command = command;
                index++;
            }
            foreach (var function in FunctionsByName)
            {
                if (!CommandsTable.TryGetValue(function.Value.VMCodeOffset, out var command))
                {
                    throw new Exception("Function command ptr incorrect");
                }
                function.Value.Command = command;
            }
            foreach (var label in LabelsByName)
            {
                if (!CommandsTable.TryGetValue(label.Value.VMCodeOffset, out var command))
                {
                    throw new Exception("Label command ptr incorrect");
                }
                label.Value.Command = command;
            }
            //foreach (var str in DataStrings)
            //{
            //    if (!StringsByOffset.ContainsKey(str.Offset+VMDataOffset) && str.Text != "[EMPTY]")
            //    {
            //        throw new Exception("Some content string doesn't used. May be error in code");
            //    }
            //}
            return v;
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
    }
}
