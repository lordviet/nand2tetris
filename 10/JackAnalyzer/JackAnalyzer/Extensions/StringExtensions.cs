namespace JackAnalyzer.Extensions
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

        public static string ConstructSymbolNode(this string source)
        {
            // TODO: if source is symbol, what is a symbol
            return source.ConstructNode("symbol");
        }

        public static string ConstructKeywordNode(this string source)
        {
            return source.ConstructNode("keyword");
        }

        private static string ConstructNode(this string source, string tagName)
        {
            return $"<{tagName}> {source} </{tagName}>";
        }
    }
}

