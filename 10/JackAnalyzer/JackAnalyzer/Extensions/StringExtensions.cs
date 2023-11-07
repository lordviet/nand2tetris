using JackAnalyzer.Enums;
using static JackAnalyzer.Constants;

namespace JackAnalyzer.Extensions
{
    public static class StringExtensions
    {
        public static string StripComment(this string source)
        {
            string forwardSlashSyntax = "//";
            string slashStarSyntax = "/*";
            string starSyntax = "*";

            string initialStrip = source.StripCommentCore(forwardSlashSyntax);
            string secondaryStrip = initialStrip.StripCommentCore(slashStarSyntax);
            return secondaryStrip.TrimStart().IndexOf("*") == 0 ? secondaryStrip.StripCommentCore(starSyntax) : secondaryStrip;
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
            if (LexicalElements.SpecialSymbolOutputMap.TryGetValue(source, out string? specialSymbolOutput))
            {
                return specialSymbolOutput.ConstructNode(Tags.Symbol);
            }

            return source.ConstructNode(Tags.Symbol);
        }

        public static string ConstructKeywordNode(this string source)
        {
            return source.ConstructNode(Tags.Keyword);
        }

        public static string ConstructIdentifierNode(this string source)
        {
            return source.ConstructNode(Tags.Identifier);
        }

        public static string ConstructIntegerConstantNode(this string source)
        {
            return source.ConstructNode(Tags.IntegerConstant);
        }

        public static string ConstructStringConstantNode(this string source)
        {
            // NOTE: Foolish implementation
            return source.Trim('"').ConstructNode(Tags.StringConstant);
        }

        public static string ConstructKeywordConstantNode(this string source)
        {
            return source.ConstructNode(Tags.KeywordConstant);
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

            return source.IsInCollection(LexicalElements.KeywordMap, constants);
        }

        public static bool IsOp(this string source)
        {
            char[] operators = new char[]
            {
                '+', '-', '*', '/', '&', '|', '<', '>', '='
            };

            return source.IsInCollection(LexicalElements.SymbolMap, operators);
        }

        public static bool IsUnaryOp(this string source)
        {
            char[] operators = new char[]
            {
                '-', '~'
            };

            return source.IsInCollection(LexicalElements.SymbolMap, operators);
        }

        public static bool IsInCollection<T>(this string source, Dictionary<string, T> referenceMap, T[] collection)
        {
            if (referenceMap.TryGetValue(source, out T? value))
            {
                return collection.Contains(value);
            }

            return false;
        }
    }
}
