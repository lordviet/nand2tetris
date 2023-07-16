﻿using System.Text.RegularExpressions;
using HackAssembler.Contracts;
using HackAssembler.Enums;
using HackAssembler.Exceptions;
using HackAssembler.Extensions;

namespace HackAssembler.Implementations
{
    public class Parser : IParser
    {
        private readonly string[] fileContents;
        private int counter;

        private readonly string parenthesesPattern = @"(?<=\\().+?(?=\\))\";

        public Parser(string fileContents)
        {
            this.fileContents = fileContents
                .Split("\n")
                .Where(line => !line.IsComment())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.TrimEnd('\r', '\n'))
                .Select(line => line.Trim())
                .ToArray();

            this.counter = 0;
        }

        public string GetCurrentInstruction()
        {
            return this.fileContents[this.counter];
        }

        public bool HasMoreCommands()
        {
            return this.counter <= this.fileContents.Length - 1;
        }

        public void Advance()
        {
            this.counter++;
        }

        public CommandType CommandType()
        {
            string command = this.GetCurrentInstruction();

            bool isACommand = command.IsACommand();
            bool isCCommand = command.IsCCommand();
            bool isLabelCommand = command.IsLabelCommand();

            return (isACommand, isCCommand, isLabelCommand) switch
            {
                (true, false, false) => Enums.CommandType.A,
                (false, true, false) => Enums.CommandType.C,
                (false, false, true) => Enums.CommandType.Label,

                _ => throw new NotSupportedException("Could not retrieve command type of current instruction.")
            };
        }

        public string Symbol()
        {
            string command = this.GetCurrentInstruction();

            CommandType type = this.CommandType();

            return type switch
            {
                Enums.CommandType.A => command[1..], // @XXX -> XXX
                Enums.CommandType.Label => Regex.Match(command, this.parenthesesPattern).Value, // (XXX) -> XXX

                _ => throw new UnexpectedCommandTypeException($"Unexpected command type received - '{type}'.")
            };
        }

        public string? Destination()
        {
            this.ThrowIfCommandTypeIsUnexpected(Enums.CommandType.C, $"Can only retrieve destination bits from a C-instruction.");

            string command = this.GetCurrentInstruction();

            int equalityIndex = command.IndexOf('=');

            if (equalityIndex == -1)
            {
                return null;
            }

            return command[0..equalityIndex];
        }

        public string Computation()
        {
            this.ThrowIfCommandTypeIsUnexpected(Enums.CommandType.C, $"Can only retrieve computation bits from a C-instruction.");

            string command = this.GetCurrentInstruction();

            int equalityIndex = command.IndexOf('=');
            int semiColonIndex = command.IndexOf(';');

            if (equalityIndex == -1 && semiColonIndex == -1)
            {
                throw new NotSupportedException("In a C-instruction, either the dest or jump fields may be empty, but not both");
            }

            if (equalityIndex == -1)
            {
                return command[0..semiColonIndex];
            }

            int startIndex = equalityIndex + 1;

            if (semiColonIndex == -1)
            {
                return command[startIndex..];
            }

            return command[startIndex..semiColonIndex];
        }

        public string? Jump()
        {
            this.ThrowIfCommandTypeIsUnexpected(Enums.CommandType.C, $"Can only retrieve computation bits from a C-instruction.");

            string command = this.GetCurrentInstruction();

            int semiColonIndex = command.IndexOf(';');

            if (semiColonIndex == -1)
            {
                return null;
            }

            int startIndex = semiColonIndex + 1;

            return command[startIndex..];
        }

        private void ThrowIfCommandTypeIsUnexpected(CommandType expected, string message)
        {
            CommandType type = this.CommandType();

            if (type != expected)
            {
                throw new UnexpectedCommandTypeException(message);
            }
        }
    }
}
