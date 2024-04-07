using MSELib.classes;
using System.IO;

namespace MSELib
{
    public class CallCommand:BaseCommand
    {
        private readonly CommandType commandType;
        private readonly byte? arg;
        private readonly FunctionItem functionItem;
        public CallCommand(CommandType commandType,FunctionItem functionItem, byte? arg = null)
        {
            this.commandType = commandType;
            this.functionItem = functionItem;
            this.arg = arg;
        }
        public override CommandType Type => commandType;
        public override uint Length => (uint)(base.Length + (Type == CommandType.CALL_BYTE_ID ? sizeof(byte) : sizeof(uint)) + (arg != null ? sizeof(byte) : 0));
        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            if (Type == CommandType.CALL_BYTE_ID)
            {
                writer.Write((byte)functionItem.Id);
            }
            else
            {
                writer.Write(functionItem.Id);
            }
            if(arg is byte value)
            {
                writer.Write(value);
            }
        }
    }
}
