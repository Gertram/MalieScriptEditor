using MSELib.classes;
using System.IO;

namespace MSELib
{
    public class UIntArgumentCommand:BaseCommand
    {
        private readonly CommandType commandType;
        private readonly uint arg;
        public UIntArgumentCommand(CommandType commandType,uint arg)
        {
            this.commandType = commandType;
            this.arg = arg;
        }
        public override CommandType Type => commandType;
        public override uint Length => base.Length + sizeof(uint);
        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(arg);
        }
    }
}
