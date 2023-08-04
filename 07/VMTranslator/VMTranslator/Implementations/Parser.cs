﻿using VMTranslator.Contracts;
using VMTranslator.Enums;
using VMTranslator.Extensions;

namespace VMTranslator.Implementations
{
    public class Parser : IParser
    {
        private readonly string[] fileContents;
        private int counter;

        public Parser(string fileContents)
        {
            this.fileContents = PreprocessFileContents(fileContents);
            this.counter = 0;
        }

        // TODO: Possibly private, we'll see.
        public string GetCurrentInstruction()
        {
            return this.fileContents[this.counter];
        }

        public bool HasMoreCommands()
        {
            return this.counter < this.fileContents.Length;
        }

        public void Advance()
        {
            this.counter++;
        }

        public CommandType CommandType()
        {
            string currentInstruction = this.GetCurrentInstruction();

            if (currentInstruction.Contains(Constants.PushKeyword))
            {
                return Enums.CommandType.Push;
            }

            if (currentInstruction.Contains(Constants.PopKeyword))
            {
                return Enums.CommandType.Pop;
            }

            if (Constants.ArithmeticCommandKeywords.Any(currentInstruction.Contains))
            {
                return Enums.CommandType.Arithmetic;
            }

            throw new NotSupportedException($"Could not derive command type out of the following instruction '{currentInstruction}'");
        }

        public string FirstArg()
        {
            string currentInstruction = this.GetCurrentInstruction();

            CommandType currentCommandType = this.CommandType();

            return currentCommandType switch
            {
                Enums.CommandType.Arithmetic => currentInstruction, // add, sub, etc...
                Enums.CommandType.Return => throw new InvalidOperationException($"Invalid operation: Trying to access the first argument of the {currentCommandType} instruction is not valid."),
                _ => currentInstruction.ExtractFirstArgumentFromInstruction(),
            };
        }

        public int SecondArg()
        {
            string currentInstruction = this.GetCurrentInstruction();

            CommandType currentCommandType = this.CommandType();

            return currentCommandType switch
            {
                Enums.CommandType.Push or
                Enums.CommandType.Pop or
                Enums.CommandType.Function or
                Enums.CommandType.Call => currentInstruction.ExtractSecondArgumentFromInstruction(),
                _ => throw new InvalidOperationException($"Invalid operation: Trying to access the second argument of the {currentCommandType} instruction is not valid.")
            };
        }

        private static string[] PreprocessFileContents(string fileContents)
        {
            return fileContents
                .Split(Environment.NewLine)
                .Select(line => line.StripComment())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.TrimEnd('\r', '\n'))
                .Select(line => line.Trim())
                .ToArray();
        }

    }
}
