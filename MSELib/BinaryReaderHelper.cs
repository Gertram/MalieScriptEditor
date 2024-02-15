using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MSELib
{
    public static class BinaryReaderHelper
    {
       
        public static ushort PeekUInt16(this BinaryReader reader)
        {
            var value = reader.ReadUInt16();
            reader.BaseStream.Position -= sizeof(ushort);
            return value;
        }
        public static uint PeekUInt32(this BinaryReader reader)
        {
            var value = reader.ReadUInt32();
            reader.BaseStream.Position -= sizeof(uint);
            return value;
        }
        private static int GetCharsCount(string text, int min, int max)
        {
            return text.Where(e => e >= min && e <= max).Count();
        }
        public static bool ContainsJapanese(this string text, int limit = 5)
        {
            if(text == null)
            {
                return false;
            }
            var hiragana = GetCharsCount(text, 0x3040, 0x309F);
            var katakana = GetCharsCount(text, 0x30A0, 0x30FF);
            var kanji = GetCharsCount(text, 0x4E00, 0x9FBF);
            return hiragana + katakana + kanji > limit;
        }
    }
}
