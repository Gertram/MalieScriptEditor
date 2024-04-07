using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MSELib
{
    public static class BinaryReaderHelper
    {
       
        public static ushort PeekUInt16(this BinaryReader reader)
        {
            var value = reader.ReadUInt16();
            reader.BaseStream.Position -= sizeof(ushort);
            return value;
        }
        public static uint PeekUInt32(this BinaryReader reader)
        {
            var value = reader.ReadUInt32();
            reader.BaseStream.Position -= sizeof(uint);
            return value;
        }
    }
}
