using MSELib.classes;
using System.IO;

namespace MSELib
{
    public class NoArgumentCommand : BaseCommand
    {
        private readonly CommandType commandType;

        public NoArgumentCommand(CommandType commandType)
        {
            this.commandType = commandType;
        }

        public override CommandType Type => commandType;
    }
}
