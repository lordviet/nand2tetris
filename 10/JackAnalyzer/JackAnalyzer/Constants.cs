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

            public static readonly Dictionary<Keyword, string> ReverseKeywordMap = new()
            {
                { Keyword.Class, "class" },
                { Keyword.Function, "function" },
                { Keyword.Method, "method" },
                { Keyword.Constructor, "constructor" },
                { Keyword.Integer, "integer" },
                { Keyword.Boolean, "boolean" },
                { Keyword.Char, "char" },
                { Keyword.Void, "void" },
                { Keyword.Var, "var" },
                { Keyword.Static, "static" },
                { Keyword.Field, "field" },
                { Keyword.Let, "let" },
                { Keyword.Do, "do" },
                { Keyword.If, "if" },
                { Keyword.Else, "else" },
                { Keyword.While, "while" },
                { Keyword.Return, "return" },
                { Keyword.True, "true" },
                { Keyword.False, "false" },
                { Keyword.Null, "null" },
                { Keyword.This, "this" }
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

        public static class Symbols
        {
            public const string LeftParenthesis = "(";
            public const string RightParenthesis = ")";
            public const string LeftCurlyBrace = "{";
            public const string RightCurlyBrace = "}";
            public const string EqualitySign = "=";
            public const string Semicolon = ";";
        }
    }
}

