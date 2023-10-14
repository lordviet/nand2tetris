namespace JackAnalyzer.Extensions
{
    public static class StringExtensions
    {
        public static string StripComment(this string source)
        {
            string forwardSlashSyntax = "//";
            string slashStarSyntax = "/*";

            string initialStrip = source.StripCommentCore(forwardSlashSyntax);
            return initialStrip.StripCommentCore(slashStarSyntax);
        }

        public static string StripCommentCore(this string source, string commentSyntax)
        {
            int commentStart = source.IndexOf(commentSyntax);

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

        public static string ConstructIdentifierNode(this string source)
        {
            return source.ConstructNode("identifier");
        }

        private static string ConstructNode(this string source, string tagName)
        {
            return $"<{tagName}> {source} </{tagName}>";
        }

        public static string ConstructOpeningTag(this string tagName)
        {
            return $"<{tagName}>";
        }

        public static string ConstructClosingTag(this string tagName)
        {
            return $"</{tagName}>";
        }
    }
}

