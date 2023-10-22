using JackAnalyzer.Enums;

namespace JackAnalyzer.Extensions
{
    public static class KeywordExtensions
    {
        public static bool IsType(this Keyword source)
        {
            return source == Keyword.Integer || source == Keyword.Char || source == Keyword.Boolean || source == Keyword.Class;
        }
    }
}

