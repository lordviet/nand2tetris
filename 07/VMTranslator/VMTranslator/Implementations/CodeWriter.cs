using System.Text;
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
        private const string ARegEqDPlusM = "A=D+M\n";

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
                HandlePushInConstantSegment(commandType, index);

                return;
            }

            // TODO: think aboout safe checks
            string? segmentMnemonic = segment.ToSegmentMnemonic();

            if (segmentMnemonic is not null)
            {
                HandlePushPopInMemorySegment(commandType, index, segmentMnemonic);

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

            string aInstructionForStackPointer = Constants.SegmentMnemonics.StackPointer.ToAInstruction();

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(DRegEqM);

            this.DecrementStackPointerCommand();

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(DRegEqDPlusM)
                            .Append(MRegEqD);

            this.IncrementStackPointerCommand();

            return;
        }
        #endregion

        #region Push/Pop handlers
        private void HandlePushInConstantSegment(CommandType commandType, int index)
        {
            switch (commandType)
            {
                case CommandType.Push:
                    this.HandlePushInConstantSegment(index);
                    return;
                default:
                    throw new NotSupportedException($"Unexpected command type '{commandType}'! Expected '{CommandType.Push}'.");
            };

        }

        // Handles LCL, ARG, THIS, THAT
        // TODO: Possibly introduce Enum for segments
        private void HandlePushPopInMemorySegment(CommandType commandType, int index, string segmentMnemonic)
        {
            switch (commandType)
            {
                case CommandType.Push:
                    this.HandlePushInMemorySegment(index, segmentMnemonic);
                    return;
                case CommandType.Pop:
                    this.HandlePopInMemorySegment(index, segmentMnemonic);
                    return;
                default:
                    throw new NotSupportedException($"Unexpected command type '{commandType}'! Expected either '{CommandType.Push}' or '{CommandType.Pop}'.");
            };
        }

        private void HandlePushInConstantSegment(int index)
        {
            string aInstructionForIndex = $"{index}".ToAInstruction();
            string aInstructionForStackPointer = Constants.SegmentMnemonics.StackPointer.ToAInstruction();

            this.transformed.Append(aInstructionForIndex)
                            .Append(DRegEqA)
                            .Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(MRegEqD);

            this.IncrementStackPointerCommand();
        }

        private void HandlePushInMemorySegment(int index, string memorySegment)
        {
            string aInstructionForIndex = $"{index}".ToAInstruction();
            string aInstructionForSegment = memorySegment.ToAInstruction();   
            string aInstructionForStackPointer = Constants.SegmentMnemonics.StackPointer.ToAInstruction();

            // Store (LCL + Index) in D register
            this.transformed.Append(aInstructionForIndex)
                            .Append(DRegEqA)
                            .Append(aInstructionForSegment)
                            .Append(ARegEqDPlusM)   
                            .Append(DRegEqM);

            // RAM[SP] = RAM[LCL + Index]
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(MRegEqD);

            // SP++
            this.IncrementStackPointerCommand();
        }

        private void HandlePopInMemorySegment(int index, string memorySegment)
        {
            // SP--
            this.DecrementStackPointerCommand();

            string aInstructionForIndex = $"{index}".ToAInstruction();
            string aInstructionForSegment = memorySegment.ToAInstruction();
            string aInstructinoForR13 = "R13".ToAInstruction();
            string aInstructionForStackPointer = Constants.SegmentMnemonics.StackPointer.ToAInstruction();

            // Store (LCL + Index) in a free register R13
            this.transformed.Append(aInstructionForIndex)
                            .Append(DRegEqA)
                            .Append(aInstructionForSegment)
                            .Append(DRegEqDPlusM)
                            .Append(aInstructinoForR13)
                            .Append(MRegEqD);

            // Store RAM[SP] in the D register
            // Retrieve the pointer (LCL + Index) from R13
            // RAM[LCL + Index] = RAM[SP]
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(DRegEqM)
                            .Append(aInstructinoForR13)
                            .Append(ARegEqM)
                            .Append(MRegEqD);

        }
        #endregion

        #region Common commands
        private void IncrementStackPointerCommand()
        {
            this.transformed.Append(Constants.SegmentMnemonics.StackPointer.ToAInstruction())
                            .Append(MPlusOne);
        }

        private void DecrementStackPointerCommand()
        {
            this.transformed.Append(Constants.SegmentMnemonics.StackPointer.ToAInstruction())
                            .Append(MMinusOne);
        }
        #endregion
    }
}

