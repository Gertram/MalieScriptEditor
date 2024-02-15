namespace MSEGui
{
    public static class StringHelpers
    {
        public static string Escape(this string text)
        {
            if (text.Length == 0)
            {
                return "[EMPTY]";
            }
            text = text.Replace("\r", "\\r").Replace("\n", "\\n");
            return text;
        }
        public static string Unescape(this string text)
        {
            if(text == "[EMPTY]")
            {
                return "";
            }
            text = text.Replace("\\r", "\r").Replace("\\n", "\n");
            return text;
        }
    }
}