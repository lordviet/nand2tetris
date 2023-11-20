using JackCompiler.Enums;

namespace JackCompiler.Extensions
{
    public static class KeywordExtensions
    {
        public static bool IsType(this Keyword source)
        {
            return source == Keyword.Integer || source == Keyword.Char || source == Keyword.Boolean || source == Keyword.Class;
        }

        public static bool IsBeginningOfStatement(this Keyword source)
        {
            return source == Keyword.Let || source == Keyword.If || source == Keyword.While || source == Keyword.Do || source == Keyword.Return;
        }

        public static bool IsKeywordConstant(this Keyword source)
        {
            return source == Keyword.True || source == Keyword.False || source == Keyword.Null || source == Keyword.This;
        }

        public static IdentifierKind ToIdentifierKind(this Keyword source)
        {
            return source switch
            {
                Keyword.Static => IdentifierKind.Static,
                Keyword.Field => IdentifierKind.Field,

                Keyword.Var => IdentifierKind.Var,

                _ => throw new Exception($"Keyword {source} is not a valid identifier kind")
            };
        }
    }
}
