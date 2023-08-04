﻿using System.Text;
using VMTranslator.Contracts;
using VMTranslator.Enums;
using VMTranslator.Extensions;

namespace VMTranslator.Implementations
{
    public class CodeWriter : ICodeWriter
    {
        private readonly StringBuilder transformed;

        // TODO: idea make different extension methods over the string builder F(sb) => sb and chain them; document them in a human-readable way
        // TODO: introduce common commands and document
        private const string DRegEqA = "D=A\n";
        private const string DRegEqM = "D=M\n";
        private const string ARegEqM = "A=M\n";
        private const string MRegEqD = "M=D\n";
        private const string DRegEqDPlusM = "D=D+M\n";

        private const string MPlusOne = "M=M+1\n";
        private const string MMinusOne = "M=M-1\n";

        public CodeWriter()
        {
            this.transformed = new StringBuilder();
        }

        public void WriteCommentedOutInstruction(string instruction)
        {
            this.transformed.Append(instruction.CommentOut());
        }

        public void WriteArithmetic(string command)
        {
            if (!Constants.ArithmeticCommandKeywords.Contains(command))
            {
                throw new ArgumentException($"Command '{command}' is not a valid artihmetic command.");
            }

            if (command == "add")
            {
                HandleAddCommand();

                return;
            }

            throw new NotImplementedException();
        }

        public void WritePushPop(CommandType commandType, string segment, int index)
        {
            // TODO: introduce a list of valid segments to check against if the input segment is valid

            if (segment == "constant")
            {
                HandlePushPopInConstantSegment(commandType, index);

                return;
            }

            // TODO: Implement remaining segments

            throw new NotImplementedException();
        }

        public string Close()
        {
            return this.transformed.ToString();
        }

        #region Arithmetic handlers
        private void HandleAddCommand()
        {
            this.DecrementStackPointerCommand();

            string aInstructionForPointer = $"@{Constants.StackPointerMnemonic}\n";

            this.transformed.Append(aInstructionForPointer)
                            .Append(ARegEqM)
                            .Append(DRegEqM);

            this.DecrementStackPointerCommand();

            this.transformed.Append(aInstructionForPointer)
                            .Append(ARegEqM)
                            .Append(DRegEqDPlusM)
                            .Append(MRegEqD);

            this.IncrementStackPointerCommand();

            return;
        }
        #endregion

        #region Push/Pop handlers
        private void HandlePushPopInConstantSegment(CommandType commandType, int index)
        {
            switch (commandType)
            {
                case CommandType.Push:
                    this.HandlePushInConstantSegment(index);
                    return;
                case CommandType.Pop:
                    this.HandlePopInConstantSegment(index);
                    return;
                default:
                    throw new NotSupportedException($"Unexpected command type '{commandType}'! Expected either '{CommandType.Push}' or '{CommandType.Pop}'.");
            };

        }

        private void HandlePushInConstantSegment(int index)
        {
            string aInstructionForIndex = $"@{index}\n";
            string aInstructionForPointer = $"@{Constants.StackPointerMnemonic}\n";

            this.transformed.Append(aInstructionForIndex)
                            .Append(DRegEqA)
                            .Append(aInstructionForPointer)
                            .Append(ARegEqM)
                            .Append(MRegEqD);

            this.IncrementStackPointerCommand();
        }

        // TODO: think about this one?
        private void HandlePopInConstantSegment(int index)
        {
            //string aInstruction = $"@{index}\n";

            //this.transformed.Append(aInstruction)
            //                .Append(DRegEqA);

            this.DecrementStackPointerCommand();
        }
        #endregion

        private void IncrementStackPointerCommand()
        {
            string aInstruction = $"@{Constants.StackPointerMnemonic}\n";

            this.transformed.Append(aInstruction)
                            .Append(MPlusOne);
        }

        private void DecrementStackPointerCommand()
        {
            this.transformed.Append($"@{Constants.StackPointerMnemonic}\n")
                            .Append(MMinusOne);
        }
    }
}
