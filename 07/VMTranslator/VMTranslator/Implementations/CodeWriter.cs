using System.Text;
using VMTranslator.Contracts;
using VMTranslator.Enums;
using VMTranslator.Extensions;

namespace VMTranslator.Implementations
{
    public class CodeWriter : ICodeWriter
    {
        private readonly string fileName;
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

        public CodeWriter(string fileName)
        {
            this.fileName = fileName;
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
            // TODO: idea, use a mapper to convert the segment to an Enum and use a generalized version of HandlePushPopInMemorySegment
            if (segment == "constant")
            {
                HandlePushInConstantSegment(commandType, index);

                return;
            }

            if (segment == "static")
            {
                HandlePushPopInStaticSegment(commandType, index, this.fileName);

                return;
            }

            if (segment == "temp")
            {
                HandlePushPopInTempSegment(commandType, index);

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

            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

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

        private void HandlePushPopInStaticSegment(CommandType commandType, int index, string fileName)
        {
            switch (commandType)
            {
                case CommandType.Push:
                    this.HandlePushInStaticSegment(index, fileName);
                    return;
                case CommandType.Pop:
                    this.HandlePopInStaticSegment(index, fileName);
                    return;
                default:
                    throw new NotSupportedException($"Unexpected command type '{commandType}'! Expected either '{CommandType.Push}' or '{CommandType.Pop}'.");
            };
        }

        private void HandlePushPopInTempSegment(CommandType commandType, int index)
        {
            switch (commandType)
            {
                case CommandType.Push:
                    this.HandlePushInTempSegment(index);
                    return;
                case CommandType.Pop:
                    this.HandlePopInTempSegment(index);
                    return;
                default:
                    throw new NotSupportedException($"Unexpected command type '{commandType}'! Expected either '{CommandType.Push}' or '{CommandType.Pop}'.");
            };
        }

        // Handles LCL, ARG, THIS, THAT
        // TODO: Possibly introduce Enum for segments
        private void HandlePushPopInMemorySegment(CommandType commandType, int index, string segment)
        {
            switch (commandType)
            {
                case CommandType.Push:
                    this.HandlePushInMemorySegment(index, segment);
                    return;
                case CommandType.Pop:
                    this.HandlePopInMemorySegment(index, segment);
                    return;
                default:
                    throw new NotSupportedException($"Unexpected command type '{commandType}'! Expected either '{CommandType.Push}' or '{CommandType.Pop}'.");
            };
        }

        private void HandlePushInConstantSegment(int index)
        {
            string aInstructionForIndex = $"{index}".ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            this.transformed.Append(aInstructionForIndex)
                            .Append(DRegEqA)
                            .Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(MRegEqD);

            this.IncrementStackPointerCommand();
        }

        private void HandlePushInStaticSegment(int index, string fileName)
        {
            string aInstructionForStaticVariable = $"{fileName}.{index}".ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store RAM[@fileName.index] in D register
            this.transformed.Append(aInstructionForStaticVariable)
                            .Append(DRegEqM);

            // RAM[SP] = RAM[@fileName.index]
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(MRegEqD);

            this.IncrementStackPointerCommand();
        }

        private void HandlePopInStaticSegment(int index, string fileName)
        {
            // SP--
            this.DecrementStackPointerCommand();

            string aInstructionForStaticVariable = $"{fileName}.{index}".ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store RAM[SP] in the D register
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(DRegEqM);

            // RAM[@fileName.index] = RAM[SP]
            this.transformed.Append(aInstructionForStaticVariable)
                            .Append(MRegEqD);
        }

        private void HandlePushInTempSegment(int index)
        {
            string aInstructionForRegister = $"R{index + Constants.DefaultTempRegisterIndex}".ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store RAM[@R(5+index)] in D register
            this.transformed.Append(aInstructionForRegister)
                            .Append(DRegEqM);

            // RAM[SP] = RAM[@R(5+index)]
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(MRegEqD);

            this.IncrementStackPointerCommand();
        }

        private void HandlePopInTempSegment(int index)
        {
            // SP--
            this.DecrementStackPointerCommand();

            string aInstructionForRegister = $"R{index + Constants.DefaultTempRegisterIndex}".ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store RAM[SP] in the D register
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(DRegEqM);

            // RAM[@R(5+index)] = RAM[SP]
            this.transformed.Append(aInstructionForRegister)
                            .Append(MRegEqD);
        }

        private void HandlePushInMemorySegment(int index, string memorySegment)
        {
            string aInstructionForIndex = $"{index}".ToAInstruction();
            string aInstructionForSegment = memorySegment.ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

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
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

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
            this.transformed.Append(Constants.Mnemonics.StackPointer.ToAInstruction())
                            .Append(MPlusOne);
        }

        private void DecrementStackPointerCommand()
        {
            this.transformed.Append(Constants.Mnemonics.StackPointer.ToAInstruction())
                            .Append(MMinusOne);
        }
        #endregion
    }
}

