using MSELib.classes;
using System.IO;

namespace MSELib
{
    public class ByteArgumentCommand:BaseCommand
    {
        private readonly CommandType commandType;
        private readonly byte arg;
        public ByteArgumentCommand(CommandType commandType,byte arg)
        {
            this.commandType = commandType;
            this.arg = arg;
        }
        public override CommandType Type => commandType;
        public override uint Length => base.Length + sizeof(byte);
        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(arg);
        }
    }
}
