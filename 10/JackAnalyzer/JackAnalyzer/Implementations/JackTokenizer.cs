using System.Text.RegularExpressions;
using JackAnalyzer.Contracts;
using JackAnalyzer.Enums;
using JackAnalyzer.Extensions;

namespace JackAnalyzer.Implementations
{
    public class JackTokenizer : IJackTokenizer
    {
        private readonly string[] fileContents;
        private int counter;

        public JackTokenizer(string fileContents)
        {
            this.fileContents = PreprocessFileContents(fileContents);
            this.counter = 0;
        }

        public string GetCurrentToken()
        {
            return this.fileContents[this.counter];
        }

        public bool HasMoreTokens()
        {
            return this.counter < this.fileContents.Length;
        }

        public void Advance()
        {
            this.counter++;
        }

        public TokenType TokenType()
        {
            throw new NotImplementedException();
        }

        public Keyword Keyword()
        {
            if (this.TokenType() != Enums.TokenType.Keyword)
            {
                throw new Exception("Token type must be keyword");
            }


            throw new NotImplementedException();
        }

        public char Symbol()
        {
            throw new NotImplementedException();
        }

        public string Identifier()
        {
            throw new NotImplementedException();
        }

        public int IntegerValue()
        {
            throw new NotImplementedException();
        }

        public string StringValue()
        {
            throw new NotImplementedException();
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
    }
}

