﻿using System.Text.RegularExpressions;
using JackAnalyzer.Contracts;
using JackAnalyzer.Enums;
using JackAnalyzer.Extensions;

namespace JackAnalyzer.Implementations
{
    public class JackTokenizer : IJackTokenizer
    {
        private readonly string[][] fileContents;

        public JackTokenizer(string fileContents)
        {
            this.fileContents = PreprocessFileContents(fileContents);
        }

        public bool HasMoreTokens()
        {
            throw new NotImplementedException();
        }

        public void Advance()
        {
            throw new NotImplementedException();
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

        private static string[][] PreprocessFileContents(string fileContents)
        {
            return fileContents
                .Split(Environment.NewLine)
                .Select(line => line.StripComment())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.TrimEnd('\r', '\n'))
                .Select(line => line.Trim())
                .Select(line => Regex.Split(line, @"(\s+|\b|\W|[,\.])")
                                     .Where(str => !string.IsNullOrWhiteSpace(str))
                                     .ToArray())
                .ToArray();
        }

        // TODO: Handle various edge cases full strings and various symbols (look behind/ahead logic in regex)
    }
}
