using System.Collections.Generic;

namespace MSELib.classes
{
    public class CommandItem
    {
        public CommandItem(int offset, CommandType type, List<ArgumentItem> args)
        {
            Offset = offset;
            Type = type;
            Args = args;
        }

        public int Offset { get; set; }
        public CommandType Type { get; set; }
        public List<ArgumentItem> Args { get; set; }
    }
}
