using JackCompiler.Enums;
using static JackCompiler.Constants;

namespace JackCompiler.Extensions
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

        public static string ConstructFunctionName(this string className, string subroutineName)
        {
            return $"{className}.{subroutineName}";
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
