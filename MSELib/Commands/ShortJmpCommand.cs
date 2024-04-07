using MSELib.classes;
using System.IO;

namespace MSELib
{
    public class ShortJmpCommand : BaseCommand, IJmpCommand
    {
        private readonly CommandType commandType;
        private readonly byte commandOffset;
        public ShortJmpCommand(CommandType commandType,byte commandOffset)
        {
            this.commandType = commandType;
            this.commandOffset = commandOffset;
        }

        public uint CommandOffset => commandOffset;

        public BaseCommand Command { get; set; }
        public override CommandType Type => commandType;
        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Command.Offset);
        }
    }
}
