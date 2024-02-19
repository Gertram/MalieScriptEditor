using System.Collections.Generic;
using System.IO;

namespace MSELib.classes
{
    public interface IArgument
    {
        //public int Offset { get; set; }
        //public IArgumentType Type;
        //public byte ByteValue { get; set; }
        //public ushort UshortValue { get; set; }
        //public uint UintValue { get; set; }
        uint Value { get; set; }
        void Read(BinaryReader reader, IDictionary<uint,StringsItem> contents);
        void Write(BinaryWriter writer);
    }
}
