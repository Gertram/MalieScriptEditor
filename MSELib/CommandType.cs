using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSELib
{

    public enum CommandType
    {
        JMP = 0x0,
        JNZ = 0x1,
        JZ = 0x2,
        CALL_UINT_ID = 0x3,
        CALL_BYTE_ID = 0x4,
        MASK_VEIP = 0x5,
        PUSH_R32 = 0x6,
        POP_R32 = 0x7,
        PUSH_INT32 = 0x8,
        PUSH_STR_BYTE = 0x9,
        PUSH_STR_SHORT = 0xA,
        NONE = 0xB,
        PUSH_STR_INT = 0xC,
        PUSH_UINT32 = 0xD,
        POP = 0xE,
        PUSH_0 = 0xF,
        UNKNOWN_1 = 0x10,
        PUSH_0x = 0x11,
        PUSH_SP = 0x12,
        NEG = 0x13,
        ADD = 0x14,
        SUB = 0x15,
        MUL = 0x16,
        DIV = 0x17,
        MOD = 0x18,
        AND = 0x19,
        OR = 0x1A,
        XOR = 0x1B,
        NOT = 0x1C,
        BOOL1 = 0x1D,
        BOOL2 = 0x1E,
        BOOL3 = 0x1F,
        BOOL4 = 0x20,
        ISL = 0x21,
        ISLE = 0x22,
        ISNLE = 0x23,
        ISNL = 0x24,
        ISEQ = 0x25,
        ISNEQ = 0x26,
        SHL = 0x27,
        SAR = 0x28,
        INC = 0x29,
        DEC = 0x2A,
        ADD_REG = 0x2B,
        DEBUG = 0x2C,
        CALL_UINT_NO_PARAM = 0x2D,
        ADD_2 = 0x2E,
        FPCOPY = 0x2F,
        FPGET = 0x30,
        INITSTACK = 0x31,
        UNKNOWN_2 = 0x32,
        RET = 0x33,
    }
    public enum ArgumentType
    {
        BYTE = 0x0,
        SHORT = 0x1,
        INT = 0x2,
        FUNCTION_BYTE_ID = 0x3,
        FUNCTION_INT_ID = 0x4,
        STR_BYTE_ID = 0x5,
        STR_SHORT_ID = 0x6,
        STR_INT_ID = 0x7,
    }
    public class ArgumentItem
    {
        public int Offset { get; set; }
        public ArgumentType Type;
        public object Value;
    }
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
