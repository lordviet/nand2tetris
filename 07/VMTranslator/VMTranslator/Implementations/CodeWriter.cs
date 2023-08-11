using System.Text;
using VMTranslator.Contracts;
using VMTranslator.Enums;
using VMTranslator.Exceptions;
using VMTranslator.Extensions;

namespace VMTranslator.Implementations
{
    public class CodeWriter : ICodeWriter
    {
        private readonly string fileName;
        private readonly StringBuilder transformed;

        // TODO: idea make different extension methods over the string builder F(sb) => sb and chain them; document them in a human-readable way
        // TODO: introduce common command store and document
        private const string DRegEqA = "D=A\n";
        private const string DRegEqM = "D=M\n";
        private const string ARegEqM = "A=M\n";
        private const string MRegEqD = "M=D\n";
        private const string DRegEqDPlusM = "D=D+M\n";
        private const string DRegEqMMinusD = "D=M-D\n";
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

            if (command == "sub")
            {
                HandleSubCommand();

                return;
            }

            if (command == "neg")
            {
                HandleNegCommand();

                return;
            }

            throw new NotImplementedException();
        }

        public void WritePushPop(CommandType commandType, string segment, int index)
        {
            // TODO: use a generalized version of HandlePushPopInMemorySegment?
            Segment memorySegment = segment.ToSegmentEnum();

            switch (memorySegment)
            {
                case Segment.Constant:
                    this.HandlePushInConstantSegment(commandType, index);
                    return;
                case Segment.Static:
                    this.HandlePushPopInStaticSegment(commandType, index, this.fileName);
                    return;
                case Segment.Temp:
                    this.HandlePushPopInTempSegment(commandType, index);
                    return;
                case Segment.Pointer:
                    this.HandlePushPopInPointerSegment(commandType, index); // TODO: test this one
                    return;
                default:
                    this.HandlePushPopInMemorySegment(commandType, index, memorySegment.ToSegmentMnemonic());
                    return;
            }
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

        private void HandleSubCommand()
        {
            this.DecrementStackPointerCommand();

            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(DRegEqM);

            this.DecrementStackPointerCommand();

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(DRegEqMMinusD)
                            .Append(MRegEqD);

            this.IncrementStackPointerCommand();

            return;
        }

        private void HandleNegCommand()
        {
            this.DecrementStackPointerCommand();

            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(MMinusOne);

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
                    ThrowExpectedPushCommandTypeException(commandType);
                    return;
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
                    ThrowExpectedPushOrPopCommandTypeException(commandType);
                    return;
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
                    ThrowExpectedPushOrPopCommandTypeException(commandType);
                    return;
            };
        }

        private void HandlePushPopInPointerSegment(CommandType commandType, int index)
        {
            if (!index.IsValidPointerIndex())
            {
                throw new ArgumentException($"Unexpected index for pointer command '{index}'! Expected either '0' or '1'");
            }

            switch (commandType)
            {
                case CommandType.Push:
                    this.HandlePushInPointerSegment(index);
                    return;
                case CommandType.Pop:
                    this.HandlePopInPointerSegment(index);
                    return;
                default:
                    ThrowExpectedPushOrPopCommandTypeException(commandType);
                    return;
            };
        }

        // Handles LCL, ARG, THIS, THAT
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
                    ThrowExpectedPushOrPopCommandTypeException(commandType);
                    return;
            };
        }

        private void HandlePushInConstantSegment(int index)
        {
            string aInstructionForIndex = $"{index}".ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store @index in D register
            this.transformed.Append(aInstructionForIndex)
                            .Append(DRegEqA);

            // RAM[SP] = index
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(MRegEqD);

            // SP++
            this.IncrementStackPointerCommand();
        }

        // TODO: HandlePushPop in Static and Temp Segments is identical and can be abstracted further
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

        private void HandlePushInPointerSegment(int index)
        {
            string aInstructionForPointer = index.PointerIndexToMnemonicMemorySegment().ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store THIS/THAT in D register
            this.transformed.Append(aInstructionForPointer)
                            .Append(DRegEqA);

            // RAM[SP] = THIS/THAT (stored in D register)
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(DRegEqM);

            // SP++
            this.IncrementStackPointerCommand();
        }

        private void HandlePopInPointerSegment(int index)
        {
            // SP--
            this.DecrementStackPointerCommand();

            string aInstructionForPointer = index.PointerIndexToMnemonicMemorySegment().ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store RAM[SP] in the D register
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(DRegEqM);

            // THIS/THAT = RAM[SP]
            this.transformed.Append(aInstructionForPointer)
                            .Append(MRegEqD);
        }

        private void HandlePushInMemorySegment(int index, string segmentMnemonic)
        {
            string aInstructionForIndex = $"{index}".ToAInstruction();
            string aInstructionForSegment = segmentMnemonic.ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store (LCL|ARG|THIS|THAT + Index) in D register
            this.transformed.Append(aInstructionForIndex)
                            .Append(DRegEqA)
                            .Append(aInstructionForSegment)
                            .Append(ARegEqDPlusM)
                            .Append(DRegEqM);

            // RAM[SP] = RAM[LCL|ARG|THIS|THAT + Index]
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(ARegEqM)
                            .Append(MRegEqD);

            // SP++
            this.IncrementStackPointerCommand();
        }

        private void HandlePopInMemorySegment(int index, string segmentMnemonic)
        {
            // SP--
            this.DecrementStackPointerCommand();

            string aInstructionForIndex = $"{index}".ToAInstruction();
            string aInstructionForSegment = segmentMnemonic.ToAInstruction();
            string aInstructinoForR13 = "R13".ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store (LCL|ARG|THIS|THAT + Index) in a free register R13
            this.transformed.Append(aInstructionForIndex)
                            .Append(DRegEqA)
                            .Append(aInstructionForSegment)
                            .Append(DRegEqDPlusM)
                            .Append(aInstructinoForR13)
                            .Append(MRegEqD);

            // Store RAM[SP] in the D register
            // Retrieve the pointer (LCL|ARG|THIS|THAT + Index) from R13
            // RAM[LCL|ARG|THIS|THAT + Index] = RAM[SP]
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

        #region Errors
        private static void ThrowExpectedPushOrPopCommandTypeException(CommandType unexpected)
        {
            throw new UnexpectedCommandTypeException($"Unexpected command type '{unexpected}'! Expected either '{CommandType.Push}' or '{CommandType.Pop}'.");
        }

        private static void ThrowExpectedPushCommandTypeException(CommandType unexpected)
        {
            throw new UnexpectedCommandTypeException($"Unexpected command type '{unexpected}'! Expected '{CommandType.Push}'.");
        }
        #endregion
    }
}

