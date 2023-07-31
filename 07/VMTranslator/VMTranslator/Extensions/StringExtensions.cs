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
    }
}

