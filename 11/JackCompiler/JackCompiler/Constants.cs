using JackCompiler.Enums;
using JackCompiler.Models;

namespace JackCompiler
{
    public static class Constants
    {
        public const string DefaultInputFileExtension = ".jack";
        public const string DefaultOutputFileExtension = ".vm";

        public static class LexicalElements
        {
            public static readonly Dictionary<string, Keyword> KeywordMap = new()
            {
                { "class", Keyword.Class },
                { "function", Keyword.Function },
                { "method", Keyword.Method },
                { "constructor", Keyword.Constructor },
                { "int", Keyword.Integer },
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
                { Keyword.Integer, "int" },
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

            public static readonly Dictionary<char, Command> NonUnaryOpSymbolCommandMap = new()
            {
                { '+', Command.Add },
                { '-', Command.Sub },
                { '&', Command.And },
                { '|', Command.Or },
                { '<', Command.Lt },
                { '>', Command.Gt },
                { '=', Command.Eq},
            };
        }

        public static class Symbols
        {
            public const string LeftParenthesis = "(";
            public const string RightParenthesis = ")";
            public const string LeftCurlyBrace = "{";
            public const string RightCurlyBrace = "}";
            public const string LeftSquareBracket = "[";
            public const string RightSquareBracket = "]";
            public const string EqualitySign = "=";
            public const string Semicolon = ";";
            public const string Comma = ",";
            public const string Dot = ".";
        }

        public static class Labels
        {
            public static class If
            {
                public const string True = "IF_TRUE";
                public const string False = "IF_FALSE";
                public const string End = "IF_END";
            }

            public static class While
            {
                public const string Expression = "WHILE_EXP";
                public const string End = "WHILE_END";
            }
        }

        public static class OS
        {
            public static class Math
            {
                private const int ArithmeticOperationParameters = 2;

                public static readonly OSLibMethod Multiply = new("Math.multiply", ArithmeticOperationParameters);
                public static readonly OSLibMethod Divide = new("Math.divide", ArithmeticOperationParameters);
            }

            public static class String
            {
                public static readonly OSLibMethod New = new("String.new", 1);
                public static readonly OSLibMethod AppendChar = new("String.appendChar", 2);
            }

            public static class Memory
            {
                public static readonly OSLibMethod Alloc = new("Memory.alloc", 1);
            }
        }
    }
}

