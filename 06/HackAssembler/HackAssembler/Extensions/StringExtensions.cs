namespace HackAssembler.Extensions
{
    public static class StringExtensions
    {
        public static bool IsACommand(this string source)
        {
            return source.Length > 1 && source.StartsWith("@");
        }

        public static bool IsCCommand(this string source)
        {
            char[] commandChars = new char[] { '=', ';', '+', '-', '!', '&', '|' };

            return commandChars.Any(source.Contains);
        }

        public static bool IsLabelCommand(this string source)
        {
            return source.StartsWith('(') && source.EndsWith(')');
        }

        public static string StripComment(this string source)
        {
            int commentStart = source.IndexOf("//");

            return commentStart == -1
                ? source
                : source[0..commentStart];
        }
    }
}

