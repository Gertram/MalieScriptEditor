using MSELib.classes;
using System.IO;

namespace MSELib
{
    public class JmpCommand : BaseCommand, IJmpCommand
    {
        private readonly CommandType commandType;
        private readonly uint commandOffset;
        public JmpCommand(CommandType commandType, uint commandOffset)
        {
            this.commandType = commandType;
            this.commandOffset = commandOffset;
        }

        public uint CommandOffset => commandOffset;

        public BaseCommand Command { get; set; }

        public override CommandType Type => commandType;
        public override uint Length => base.Length + sizeof(uint);
        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((Command.Offset & 0xffffff) | (CommandOffset&0x0f000000));
        }
    }
}
