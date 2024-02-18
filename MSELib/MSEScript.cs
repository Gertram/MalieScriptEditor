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
using System.Text.RegularExpressions;

namespace MSELib
{
    public enum StringType
    {
        Voice,
        Text
    }
    public class FunctionItem
    {
        public string Name { get; set; }
        public uint Id { get; set; }
        public uint VMCodeOffset { get; set; }
        public uint Reserved0 { get; set; }
    }
    public class LabelItem
    {
        public string Name { get; set; }
        public uint VMCodeOffset { get; set; }
    }
    public class MSEScript
    {
        public int Magic { get; set; }
        public List<ContentItem> ContentItems { get; set; }
        private Dictionary<string, FunctionItem> Functions { get; set; }
        private Dictionary<string, LabelItem> Labels { get; set; }
        private long ContentsOffset { get; set; }
        private byte[] Contents { get; set; }
        public byte[] VMCode { get; set; }
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
        private void GetNext(BinaryReader reader)
        {
            var flag = reader.ReadUInt32();
            if (flag > 0)
            {
                reader.ReadUInt32();
                GetNext(reader);
            }
        }
        private void ReadVars(BinaryReader reader)
        {
            var count = reader.ReadUInt32();
            for (int i = 0; i < count; i++)
            {
                var length = reader.ReadUInt32();
                if ((length >> 24) != 0x80)
                {
                    throw new Exception();
                }
                length = length & 0x7fFFFFFF;
                reader.ReadBytes((int)length);
                GetNext(reader);
                for (int j = 0; j < 4; j++)
                {
                    reader.ReadUInt32();
                }
            }
        }
        private void ReadFunctions(BinaryReader reader)
        {
            reader.ReadUInt32();
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
        private void ReadContents(BinaryReader reader)
        {

            bool is_continue = true;
            var sectionSize = reader.ReadUInt32();
            ContentsOffset = reader.BaseStream.Position;
            Contents = reader.ReadBytes((int)sectionSize);
            //ContentItems = new List<ContentItem>();
            //reader.BaseStream.Position += sizeof(ushort);
            //while (is_continue)
            //{
            //    var start = reader.BaseStream.Position;
            //    var texts = new List<string>();
            //    while (true)
            //    {
            //        var temp = reader.ReadUInt16();
            //        if (temp == 0)
            //        {
            //            var end = reader.BaseStream.Position;
            //            reader.BaseStream.Position = start;
            //            var bytes = reader.ReadBytes((int)(end - start));
            //            var text = Encoding.Unicode.GetString(bytes).TrimEnd('\0');
            //            texts.Add(text);
            //            if (text == "シナリオ終了\n")
            //            {
            //                is_continue = false;
            //                break;
            //            }
            //            start = reader.BaseStream.Position;
            //        }
            //        if (temp == 0x25a0)
            //        {
            //            break;
            //        }
            //    }
            //    ContentItems.Add(new ContentItem
            //    {
            //        Title = new StringsItem(texts[0]),
            //        Offset = (uint)start,
            //        Texts = new List<StringsItem>(texts.Skip(1).Select(x=>new StringsItem(x)))
            //    });
            //};
        }
        class MalieMoji
        {
            public string Name { get; set; }
            public uint Index { get; set; }
        }
        private void ReadCode(BinaryReader reader)
        {
            var rawLength = reader.ReadInt32();
            var end = reader.BaseStream.Position + rawLength;
            using (var writer = new StreamWriter("disasm.txt"))
            {
                try
                {
                    while (reader.BaseStream.Position != end)
                    {
                        var code = ParseCode(reader, writer);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            //VMCode = reader.ReadBytes(rawLength);
            //var offset = Functions["maliescenario"].VMCodeOffset + reader.BaseStream.Position;
            //reader.BaseStream.Position = offset;
            //var current = offset;
            //var _ms_message = Functions["_ms_message"].Id;
            //var MALIE_NAME = Functions["MALIE_NAME"].Id;
            //var MALIE_LABLE = Functions["MALIE_LABLE"].Id;
            //var tag = Functions["tag"].Id;
            //var FrameLayer_SendMessage = Functions["FrameLayer_SendMessage"].Id;
            //var System_GetResult = Functions["System_GetResult"].Id;
            //var jmpTable = new List<uint>();
            //var selectTable = new List<uint>();
            //int it = 0;
            //var chapterIndex = new List<int>();
            //var v = new List<MalieMoji>();
            //var moji = new MalieMoji();
            //using (var writer = new StreamWriter(new BufferedStream(File.OpenWrite("disasm.txt"))))
            //{
            //for (; VMCode[current] != 0x33;)
            //{
            //writer.Write($"{offset.ToString("X").PadLeft(6,'0')}  ");
            //var code = ParseCode(ref offset, reader,writer);
            //if(jmpTable.Count > 0)
            //{
            //    if(it != jmpTable.Count && offset > jmpTable[it])
            //    {
            //        it++;
            //        chapterIndex.Add(v.Count);
            //    }
            //}

            //if (code == 0x2D || code == 0x4 || code == 3)//vcall
            //{
            //    if (Parma == tag)
            //    {
            //        //var pos = lastString.IndexOf("<chapter");
            //        //if(pos != -1)
            //        //{
            //        //    var regex = new Regex(@"<chapter name='(?<title>.*)'>");
            //        //    var match = regex.Match(lastString);
            //        //    var title = match.Groups["title"].Value;
            //        //}
            //        //if (pTag)
            //        //{
            //        //    swscanf(pLastString, L"<chapter name='%ls'>", Title);
            //        //    pTag = wcschr(Title, L'\'');
            //        //    if (pTag)
            //        //    {
            //        //        *pTag = 0;
            //        //        fwprintf(stderr, L"Title found: %ls\r\n", Title);
            //        //        chapterName.push_back(Title);
            //        //    }
            //        //    chapterIndex.push_back(v.size());
            //        //    //					fprintf(stdout,"%ls\n",Title);
            //        //}
            //    }
            //    else if(Parma == _ms_message)
            //    {
            //        vmStack.Pop();
            //        moji.Index = vmStack.Pop() & ~0x80000000;
            //        while(vmStack.Count != 0)
            //        {
            //            vmStack.Pop();
            //        }
            //        vmStack.Push(0);
            //        //v.Add(moji);
            //        //moji = new MalieMoji();
            //        //selectTable.Clear();
            //    }
            //    else if(Parma == MALIE_NAME)
            //    {
            //        moji.Name = lastString;
            //    }
            //    else if(Parma == MALIE_LABLE && v.Count == 0)
            //    {
            //        if(lastString != "_link")
            //        {
            //            //szCode += ParseJmpTable(jmpTable);
            //            //it = jmpTable.begin();
            //            //offset += szCode;
            //            //continue;
            //        }
            //    }
            //    else if(Parma == System_GetResult)
            //    {
            //        foreach(var x in selectTable)
            //        {
            //            writer.WriteLine($"Select {x}");
            //        }
            //    }
            //    else if(Parma == FrameLayer_SendMessage && vmStack.Count > 4)
            //    {
            //        vmStack.Pop();
            //        vmStack.Pop();
            //        vmStack.Pop();
            //        vmStack.Pop();
            //        uint loc = vmStack.Peek();
            //        if(loc > 0)
            //        {
            //            selectTable.Add(loc);
            //        }
            //    }
            //}
            //pCurrent += szCode;
            //offset += szCode;

            //}
            //writer.WriteLine("DONE");
            //}
        }
        //private int codeLen;
        private uint Parma;
        private Stack<uint> vmStack = new Stack<uint>();
        private string GetFuncName(uint id)
        {
            return Functions.Values.First(x => x.Id == id).Name;
        }
        public Dictionary<CommandType, List<ArgumentType>> Commands = new Dictionary<CommandType, List<ArgumentType>> 
        {

        };
        private uint ParseCode(BinaryReader reader, StreamWriter writer)
        {
            uint codeSize = 1;
            var offset = reader.BaseStream.Position;
            var code = reader.ReadByte();
            uint temp = 0;
            switch (code)
            {
                case 0x0://vJmp len:4
                    Parma = reader.ReadUInt32();
                    codeSize += sizeof(uint);
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: jmp {Parma:X}");
                    break;
                case 0x1://vJnz len:4
                    Parma = reader.ReadUInt32();
                    codeSize += sizeof(uint);
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: jnz {Parma:X}");
                    break;
                case 0x2://vJz len:4
                    Parma = reader.ReadUInt32();
                    codeSize += sizeof(uint);
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: jz {Parma:X}");
                    break;
                case 0x3://GetProcAddress len:4+1=5
                    Parma = reader.ReadUInt32();
                    temp = reader.ReadByte();
                    codeSize += sizeof(uint) + sizeof(byte);
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: call {GetFuncName(Parma)} {temp}");
                    break;
                case 0x4://GetProcAddress len:1+1=2
                    Parma = reader.ReadByte();
                    temp = reader.ReadByte();
                    codeSize += sizeof(byte) + sizeof(byte);
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: call {GetFuncName(Parma)} {temp}");
                    break;
                case 0x5://去掉最低位、 len:指令位移&0xEF(Удалить младший бит, len: смещение инструкции &0xEF)
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: mask vEip");
                    break;
                case 0x6://vPush R32 len:0
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: push R32");
                    break;
                case 0x7://vPop R32 len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: pop R32");
                    break;
                case 0x8://vPush len:4
                case 0xD://vPush len:4
                    Parma = reader.ReadUInt32();
                    vmStack.Push(Parma | 0x80000000);
                    codeSize += sizeof(uint);
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: push  {Parma:X}");
                    break;
                case 0x9:
                    {//vPushStr len:1
                        Parma = reader.ReadByte();
                        codeSize++;
                        lastString = ReadString(reader, (int)(Parma + ContentsOffset));
                        var pos = lastString.IndexOf('\n');
                        if (pos != -1)
                        {
                            //lastString[pos] = '0';
                        }
                        vmStack.Push((uint)(Parma + ContentsOffset));
                        writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: push str len1");
                        break;
                    }
                case 0xA:
                    {//vPushStr len:2
                        Parma = reader.ReadUInt16();
                        codeSize += sizeof(ushort);
                        lastString = ReadString(reader, (int)(Parma + ContentsOffset));
                        var pos = lastString.IndexOf('\n');
                        if (pos != -1)
                        {
                            //lastString[pos] = '0';
                        }
                        vmStack.Push((uint)(Parma + ContentsOffset));
                        writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: push str len2");
                        break;
                    }
                case 0xB://none len:0
                    break;
                case 0xC:
                    {//vPushStr len:4
                        Parma = reader.ReadUInt32();
                        codeSize += sizeof(ushort);
                        lastString = ReadString(reader, (int)(Parma + ContentsOffset));
                        var pos = lastString.IndexOf('\n');
                        if (pos != -1)
                        {
                            //lastString[pos] = '0';
                        }
                        vmStack.Push((uint)(Parma + ContentsOffset));
                        writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: push str len4");
                        break;
                    }
                case 0xE://vPop len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: pop");
                    break;
                case 0xF://vPush 0 len:0
                    vmStack.Push(0 | 0x80000000);
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: push 0");
                    break;
                case 0x10://无对应指令 len:0 Нет соответствующей команды
                    break;
                case 0x11://vPush len:1
                    Parma = reader.ReadByte();
                    vmStack.Push(Parma | 0x80000000);
                    codeSize++;
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: push 0x{Parma:X}");
                    break;
                case 0x12://vPush [sp] len:0
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: push [sp]");
                    break;
                case 0x13://vNeg len:0
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: neg");
                    break;
                case 0x14://vAdd len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: add");
                    break;
                case 0x15://vSub len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: sub");
                    break;
                case 0x16://vMul len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: mul");
                    break;
                case 0x17://vDiv len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: div");
                    break;
                case 0x18://vMod len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: mod");
                    break;
                case 0x19://vAnd len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: and");
                    break;
                case 0x1A://vOr len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: or");
                    break;
                case 0x1B://vXor len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: xor");
                    break;
                case 0x1C://vNot len:0
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: not");
                    break;
                case 0x1D://vBOOL(param) len:0
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: BOOL(param)");
                    break;
                case 0x1E://vBOOL(param1&&param2) len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: BOOL(param1&&param2)");
                    break;
                case 0x1F://vBOOL(param1||param2)
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: BOOL(param1||param2)");
                    break;
                case 0x20://!vBOOL(param) len:0
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: !BOOL(param)");
                    break;
                case 0x21://vIsL len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: IsL");
                    break;
                case 0x22://vIsLE len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: IsLE");
                    break;
                case 0x23://vIsNLE len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: IsNLE");
                    break;
                case 0x24://vIsNL len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: IsNL");
                    break;
                case 0x25://vIsEQ len:0
                    vmStack.Pop();
                    writer.Write($"{offset.ToString("X").PadLeft(6, '0')}: IsEQ");
                    break;
                case 0x26://vIsNEQ len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: IsNEQ");
                    break;
                case 0x27://vShl len:0
                          //vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: shl");
                    break;
                case 0x28://vSar len:0
                    vmStack.Pop();
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: sar");
                    break;
                case 0x29://vInc len:0
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: inc");
                    break;
                case 0x2A://vDec len:0
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: dec");
                    break;
                case 0x2B://vAddReg len:0
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: AddReg");
                    break;
                case 0x2C://Debug len:0
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: Debug");
                    break;
                case 0x2D://vCall len:4
                    Parma = reader.ReadUInt32();
                    codeSize += sizeof(uint);
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: call {GetFuncName(Parma)}");
                    vmStack.Push(0 | 0x80000000);//ret val
                    break;
                case 0x2E://vAdd len:0
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: add");
                    break;
                case 0x2F://vFPCopy len:0
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: FPCopy");
                    break;
                case 0x30://vFPGet len:0
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: FPGet");
                    break;
                case 0x31://vPush N len:4
                    Parma = reader.ReadUInt32();
                    codeSize += sizeof(uint);
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: initStack {Parma:X}");
                    break;
                case 0x32://vJmp len:1
                    //Parma = reader.ReadByte();
                    codeSize++;
                    //writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: jmp short {Parma:X}");
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: unknown byte command {code:X}");
                    break;
                case 0x33://vJmp len:1
                    Parma = reader.ReadByte();
                    codeSize++;
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: ret {Parma:X}");
                    break;
                default:
                    writer.WriteLine($"{offset.ToString("X").PadLeft(6, '0')}: Unknow opcode {code}");
                    throw new NotImplementedException();
                    break;
            }
            //return codeSize;
            return code;
        }
        private string lastString;
        private SortedSet<int> StringOffsets { get; set; } = new SortedSet<int>();
        private string ReadString(BinaryReader reader, int offset)
        {
            var start = reader.BaseStream.Position;
            reader.BaseStream.Position = offset;
            StringOffsets.Add(offset);
            var builder = new StringBuilder();
            while (true)
            {
                var temp = reader.ReadUInt16();
                if (temp == 0)
                {
                    break;
                }
                char character = (char)temp;
                builder.Append(character);
            }
            reader.BaseStream.Position = start;
            return builder.ToString();
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
        public void WriteTitles(BinaryWriter writer)
        {
            //foreach(var titleItem in TitleItems)
            //{
            //    var title = titleItem.Title + "\0";
            //    var bytes = Encoding.Unicode.GetBytes(title);
            //    writer.Write((ushort)bytes.Length);
            //    writer.Write((ushort)0x8000);
            //    writer.Write(bytes);
            //    foreach(var parameter in titleItem.Parameters)
            //    {
            //        writer.Write(parameter);
            //    }
            //}
        }
        public void WriteContents(BinaryWriter writer)
        {
            var startPos = writer.BaseStream.Position;

            foreach (var contentItem in ContentItems)
            {
                writer.Write((ushort)0x25a0);
                foreach (var stringItem in contentItem.Texts.Prepend(contentItem.Title))
                {
                    var line = stringItem.Dump() + "\0";
                    var bytes = Encoding.Unicode.GetBytes(line);
                    writer.Write(bytes);
                }
            }
            var bytesLength = writer.BaseStream.Position - startPos;
            writer.BaseStream.Position = startPos;
            writer.Write((int)bytesLength);
            foreach (var contentItem in ContentItems)
            {
                writer.Write((ushort)0x25a0);
                foreach (var stringItem in contentItem.Texts.Prepend(contentItem.Title))
                {
                    var line = stringItem.Dump() + "\0";
                    var bytes = Encoding.Unicode.GetBytes(line);
                    writer.Write(bytes);
                }
            }
        }
        public void WriteRaw(BinaryWriter writer)
        {
            writer.Write(VMCode.Length);
            writer.Write(VMCode);
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
                writer.Write(Magic);
                WriteTitles(writer);
                WriteContents(writer);
                WriteRaw(writer);
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
