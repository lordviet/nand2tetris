using System;
namespace HackAssembler.Extensions
{
    public static class StringExtensions
    {
        public static bool IsComment(this string source)
        {
            char slash = '/';

            return source.Length >= 2 && source[0] == slash && source[1] == slash;
        }

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
    }
}

