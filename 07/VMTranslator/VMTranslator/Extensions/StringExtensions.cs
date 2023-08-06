namespace VMTranslator.Extensions
{
    public static class StringExtensions
    {
        public static string StripComment(this string source)
        {
            int commentStart = source.IndexOf("//");

            return commentStart == -1
                ? source
                : source[0..commentStart];
        }

        public static string ExtractFirstArgumentFromInstruction(this string source)
        {
            return source.Split(" ")[1];
        }

        public static int ExtractSecondArgumentFromInstruction(this string source)
        {
            string extracted = source.Split(" ").Last();

            return int.TryParse(extracted, out int parsedArg)
                ? parsedArg
                : int.MinValue;
        }

        public static string CommentOut(this string source)
        {
            return $"// {source}\n";
        }

        public static string ToAInstruction(this string source)
        {
            return $"@{source}\n";
        }
    }
}

