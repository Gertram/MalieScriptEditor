using MSELib.classes;
using System.IO;

namespace MSELib
{
    public abstract class BaseCommand
    {
        private uint offset;
        public uint Offset
        {
            get => offset;
            set
            {
                offset = value;
            }
        }
        public abstract CommandType Type { get; }
        public virtual uint Length => sizeof(byte);

        public virtual void Write(BinaryWriter writer)
        {
            var code = (byte)Type;
            writer.Write(code);
        }
    }
}
