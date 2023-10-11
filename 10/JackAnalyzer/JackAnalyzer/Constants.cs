using JackAnalyzer.Enums;

namespace JackAnalyzer
{
    public static class Constants
    {
        public const string DefaultInputFileExtension = ".jack";
        public const string DefaultOutputFileExtension = ".xml";

        public static class LexicalElements
        {
            public static readonly Dictionary<string, Keyword> KeywordMap = new()
            {
                { "class", Keyword.Class },
                { "function", Keyword.Function },
                { "method", Keyword.Method },
                { "constructor", Keyword.Constructor },
                { "integer", Keyword.Integer },
                { "boolean", Keyword.Boolean },
                { "char", Keyword.Char },
                { "void", Keyword.Void },
                { "var", Keyword.Var },
                { "static", Keyword.Static },
                { "field", Keyword.Field },
                { "let", Keyword.Let },
                { "do", Keyword.Do },
                { "if", Keyword.If },
                { "else", Keyword.Else },
                { "while", Keyword.While },
                { "return", Keyword.Return },
                { "true", Keyword.True },
                { "false", Keyword.False },
                { "null", Keyword.Null },
                { "this", Keyword.This }
            };

            public static readonly Dictionary<string, char> SymbolMap = new()
            {
                { "{", '{' },
                { "}", '}' },
                { "(", '(' },
                { ")", ')' },
                { "[", '[' },
                { "]", ']' },
                { ".", '.' },
                { ",", ',' },
                { ";", ';' },
                { "+", '+' },
                { "-", '-' },
                { "*", '*' },
                { "/", '/' },
                { "&", '&' },
                { "|", '|' },
                { "<", '<' },
                { ">", '>' },
                { "=", '=' },
                { "~", '~' }
            };

            public const string StringPattern = @"^""[^""]*""";

            public const string IdentifierPattern = @"\b[a-zA-Z_][a-zA-Z0-9_]*\b";
        }
    }
}

