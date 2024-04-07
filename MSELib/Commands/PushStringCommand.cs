using MSELib.classes;
using System.IO;

namespace MSELib
{
    public class PushStringCommand:BaseCommand
    {
        private readonly StringItem stringItem;
        private CommandType commandType;
        public PushStringCommand(CommandType commandType, StringItem stringItem)
        {
            this.commandType = commandType;
            this.stringItem = stringItem;
        }
        public override CommandType Type => commandType;
        public override uint Length
        {
            get
            {
                var offset = stringItem.Offset;
                if (commandType == CommandType.PUSH_STR_BYTE)
                {
                    if (offset > 0xFF)
                    {
                        if (offset > 0xFFFF)
                        {
                            commandType = CommandType.PUSH_STR_INT;
                            return base.Length + sizeof(uint);
                        }
                        commandType = CommandType.PUSH_STR_SHORT;
                        return base.Length + sizeof(ushort);

                    }
                    return base.Length + sizeof(byte);
                }
                if (commandType == CommandType.PUSH_STR_SHORT)
                {
                    if (offset > 0xFFFF)
                    {
                        commandType = CommandType.PUSH_STR_INT;
                        return base.Length + sizeof(uint);
                    }
                    return base.Length + sizeof(ushort);
                }
                return base.Length + sizeof(uint);
            }
        }
        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);

            if(commandType == CommandType.PUSH_STR_BYTE)
            {
                writer.Write((byte)stringItem.Offset);
            }
            else if(commandType == CommandType.PUSH_STR_SHORT)
            {
                writer.Write((ushort)stringItem.Offset);
            }
            else
            {
                writer.Write(stringItem.Offset);
            }

        }
    }
}
