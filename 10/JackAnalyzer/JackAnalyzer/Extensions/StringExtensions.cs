using JackAnalyzer.Enums;

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

        // TODO: the hard-coded strings should come from the constants
        public static string ConstructSymbolNode(this string source)
        {
            if (Constants.LexicalElements.SpecialSymbolOutputMap.TryGetValue(source, out string? specialSymbolOutput))
            {
                return specialSymbolOutput.ConstructNode("symbol");
            }

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

        public static string ConstructIntegerConstantNode(this string source)
        {
            return source.ConstructNode("integerConstant");
        }

        public static string ConstructStringConstantNode(this string source)
        {
            // TODO: Potentially problematic
            return source.Trim('"').ConstructNode("stringConstant");
        }

        public static string ConstructKeywordConstantNode(this string source)
        {
            return source.ConstructNode("keywordConstant");
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

        public static bool IsKeywordConstant(this string source)
        {
            Keyword[] constants = new Keyword[] {
                Keyword.True,
                Keyword.False,
                Keyword.Null,
                Keyword.This
            };

            return source.IsInCollection(Constants.LexicalElements.KeywordMap, constants);
        }

        public static bool IsOp(this string source)
        {
            char[] operators = new char[]
            {
                '+', '-', '*', '/', '&', '|', '<', '>', '='
            };

            return source.IsInCollection(Constants.LexicalElements.SymbolMap, operators);
        }

        public static bool IsUnaryOp(this string source)
        {
            char[] operators = new char[]
            {
                '-', '~'
            };

            return source.IsInCollection(Constants.LexicalElements.SymbolMap, operators);
        }

        public static bool IsInCollection<T>(this string source, Dictionary<string, T> referenceMap, T[] collection)
        {
            //if (!map.ContainsKey(source))
            //{
            //    return false;
            //}

            //return constants.Contains(map[source]);

            if (referenceMap.TryGetValue(source, out T? value))
            {
                return collection.Contains(value);
            }

            return false;
        }
    }
}
