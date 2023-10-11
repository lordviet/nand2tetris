using System.Text.RegularExpressions;
using JackAnalyzer.Contracts;
using JackAnalyzer.Enums;
using JackAnalyzer.Exceptions;
using JackAnalyzer.Extensions;

namespace JackAnalyzer.Implementations
{
    public partial class JackTokenizer : IJackTokenizer
    {
        private readonly string[] fileContents;
        private int counter;

        public JackTokenizer(string fileContents)
        {
            this.fileContents = PreprocessFileContents(fileContents);
            counter = 0;
        }

        public string GetCurrentToken()
        {
            return fileContents[counter];
        }

        public bool HasMoreTokens()
        {
            return counter < fileContents.Length;
        }

        public void Advance()
        {
            counter++;
        }

        public TokenType TokenType()
        {
            if (!HasMoreTokens())
            {
                throw new InvalidOperationException("There are no more tokens to process.");
            }

            string currentToken = GetCurrentToken();

            if (Constants.LexicalElements.KeywordMap.ContainsKey(currentToken))
            {
                return Enums.TokenType.Keyword;
            }

            if (Constants.LexicalElements.SymbolMap.ContainsKey(currentToken))
            {
                return Enums.TokenType.Symbol;
            }

            if (StringRegex().IsMatch(currentToken))
            {
                return Enums.TokenType.StringConstant;
            }

            if (short.TryParse(currentToken, out short _))
            {
                return Enums.TokenType.IntegerConstant;
            }

            if (IdentifierRegex().IsMatch(currentToken))
            {
                return Enums.TokenType.Identifier;
            }

            throw new InvalidTokenException(currentToken);
        }

        public Keyword Keyword()
        {
            this.ThrowIfTokenTypeDoesNotMatchExpected(Enums.TokenType.Keyword, TokenType());

            string currentToken = GetCurrentToken();

            return Constants.LexicalElements.KeywordMap[currentToken];
        }

        public char Symbol()
        {
            this.ThrowIfTokenTypeDoesNotMatchExpected(Enums.TokenType.Symbol, TokenType());

            string currentToken = GetCurrentToken();

            return Constants.LexicalElements.SymbolMap[currentToken];
        }

        public string Identifier()
        {
            this.ThrowIfTokenTypeDoesNotMatchExpected(Enums.TokenType.Identifier, TokenType());

            throw new NotImplementedException();
        }

        public int IntegerValue()
        {
            this.ThrowIfTokenTypeDoesNotMatchExpected(Enums.TokenType.IntegerConstant, TokenType());

            string currentToken = GetCurrentToken();

            if (!short.TryParse(currentToken, out short numericCurrentToken))
            {
                throw new InvalidOperationException($"Could not parse token {currentToken} to a 16-bit numeric value");
            }

            return numericCurrentToken;
        }

        public string StringValue()
        {
            this.ThrowIfTokenTypeDoesNotMatchExpected(Enums.TokenType.IntegerConstant, TokenType());

            string currentToken = GetCurrentToken();

            return currentToken;
        }

        private void ThrowIfTokenTypeDoesNotMatchExpected(TokenType expected, TokenType received)
        {
            if (expected == received)
            {
                return;
            }

            throw new UnexpectedTokenTypeException(expected, received);
        }

        // TODO: Handle parameterList
        private static string[] PreprocessFileContents(string fileContents)
        {
            string pattern = @"(""[^""]*""|\s+|\b|\W|[,\.])";

            return fileContents
                .Split(Environment.NewLine)
                .Select(line => line.StripComment())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.TrimEnd('\r', '\n'))
                .Select(line => line.Trim())
                .Select(line => Regex.Split(line, pattern)
                                     .Where(str => !string.IsNullOrWhiteSpace(str))
                                     .ToArray())
                .SelectMany(line => line)
                .ToArray();
        }

        [GeneratedRegex("^\"[^\"]*\"")]
        private static partial Regex StringRegex();
        [GeneratedRegex("\\b[a-zA-Z_][a-zA-Z0-9_]*\\b")]
        private static partial Regex IdentifierRegex();
    }
}

