using JackAnalyzer.Enums;

namespace JackAnalyzer.Extensions
{
    public static class TokenTypeExtensions
    {
        public static string TokenTypeToTag(this TokenType source, bool isClosing = false)
        {
            string typeDecl = source switch
            {
                TokenType.Keyword => "keyword",
                _ => "unknown"
            };

            return isClosing ? typeDecl.ConstructClosingTag() : typeDecl.ConstructOpeningTag();
        }
    }
}

