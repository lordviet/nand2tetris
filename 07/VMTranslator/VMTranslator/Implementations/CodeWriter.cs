using System.Text;
using VMTranslator.Contracts;
using VMTranslator.Enums;
using VMTranslator.Exceptions;
using VMTranslator.Extensions;

using AReg = VMTranslator.Constants.RegisterCommands.A;
using DReg = VMTranslator.Constants.RegisterCommands.D;
using MReg = VMTranslator.Constants.RegisterCommands.M;

namespace VMTranslator.Implementations
{
    public class CodeWriter : ICodeWriter
    {
        private readonly string fileName;
        private readonly StringBuilder transformed;

        public CodeWriter(string fileName)
        {
            this.fileName = fileName;
            this.transformed = new StringBuilder();
        }

        public void WriteCommentedOutInstruction(string instruction)
        {
            this.transformed.Append(instruction.CommentOut());
        }

        public void WriteArithmetic(string command, int counter)
        {
            if (!Constants.ArithmeticCommandKeywords.Contains(command))
            {
                throw new ArgumentException($"Command '{command}' is not a valid artihmetic command.");
            }

            ArithmeticCommand arithmeticCommand = command.ToArithmeticCommand();

            switch (arithmeticCommand)
            {
                case ArithmeticCommand.Addition:
                    HandleAddCommand();
                    return;

                case ArithmeticCommand.Subtraction:
                    HandleSubCommand();
                    return;

                case ArithmeticCommand.Negation:
                    HandleNegCommand();
                    return;

                case ArithmeticCommand.And:
                    HandleAndCommand();
                    return;

                case ArithmeticCommand.Or:
                    HandleOrCommand();
                    return;

                case ArithmeticCommand.Not:
                    HandleNotCommand();
                    return;

                case ArithmeticCommand.Equality:
                    HandleEqCommand();
                    return;

                case ArithmeticCommand.GreaterThan:
                    HandleGreaterThanCommand(counter);
                    return;

                case ArithmeticCommand.LessThan:
                    HandleLessThanCommand(counter);
                    return;
            }
        }

        public void WritePushPop(CommandType commandType, string segment, int index)
        {
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
                    this.HandlePushPopInPointerSegment(commandType, index);
                    return;
                default:
                    this.HandlePushPopInMemorySegment(commandType, index, memorySegment.ToSegmentMnemonic());
                    return;
            }
        }

        public void WriteInit()
        {
            throw new NotImplementedException();
        }

        public void WriteLabel(string label)
        {
            this.transformed.Append(label.ToLabelSymbolDeclaration());
        }

        public void WriteGoto(string label)
        {
            string aInstructionForLabel = label.ToAInstruction();
            string unconditionalJumpCommand = "0".ToJumpCommand(Constants.Mnemonics.Jumps.Uncoditional);

            this.transformed.Append(aInstructionForLabel)
                            .Append(unconditionalJumpCommand);
        }

        public void WriteIf(string label)
        {
            this.DecrementStackPointerCommand();

            string aInstructionForLabel = label.ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            string dPostComparisonJumpCommand = "D".ToJumpCommand(Constants.Mnemonics.Jumps.NotEqToZero);

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqM);

            this.transformed.Append(aInstructionForLabel)
                            .Append(dPostComparisonJumpCommand);

            this.IncrementStackPointerCommand();
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
                            .Append(AReg.EqM)
                            .Append(DReg.EqM);

            this.DecrementStackPointerCommand();

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqDPlusM)
                            .Append(MReg.EqD);

            this.IncrementStackPointerCommand();

            return;
        }

        private void HandleSubCommand()
        {
            this.DecrementStackPointerCommand();

            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqM);

            this.DecrementStackPointerCommand();

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqMMinusD)
                            .Append(MReg.EqD);

            this.IncrementStackPointerCommand();

            return;
        }

        private void HandleNegCommand()
        {
            this.DecrementStackPointerCommand();

            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(MReg.EqMinusM);

            this.IncrementStackPointerCommand();

            return;
        }

        private void HandleNotCommand()
        {
            this.DecrementStackPointerCommand();

            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Usually to negate a two's complement number, all the bits are inverted and 1 is added to the result.
            // However, in this case only the inverted bits are going to be kept.
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqExclM) // Invert the bits
                            .Append(MReg.EqD);

            this.IncrementStackPointerCommand();

            return;
        }

        private void HandleAndCommand()
        {
            this.DecrementStackPointerCommand();

            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqM);

            this.DecrementStackPointerCommand();

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqDAndM)
                            .Append(MReg.EqD);

            this.IncrementStackPointerCommand();

            return;
        }

        private void HandleOrCommand()
        {
            this.DecrementStackPointerCommand();

            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqM);

            this.DecrementStackPointerCommand();

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqDOrM)
                            .Append(MReg.EqD);

            this.IncrementStackPointerCommand();

            return;
        }

        private void HandleEqCommand()
        {
            // SP--
            this.DecrementStackPointerCommand();

            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();
            string aInstructionForR13 = "R13".ToAInstruction();

            // Store last value from the stack in the D register
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqM);

            // SP--
            this.DecrementStackPointerCommand();

            // Get the next value from the stack
            // Subtract the value in the D register from it
            // Store the result from the subtraction in the D register
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqMMinusD);

            // Strip the value to either zero or one and negate it
            this.transformed.Append(DReg.EqExclD);

            // Store the value 1 to Register 13
            // Use it for logical AND to see if the integers are equal
            // If x - y == 0; then !0 == 1; 1 AND 1 will output true
            this.transformed.Append(aInstructionForR13)
                            .Append(MReg.EqOne)
                            .Append(DReg.EqDAndM);

            // Store the inverted result in the next address shown by the stack pointer and add 1 to retrieve actual value
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(MReg.EqExclD)
                            .Append(MReg.EqMPlusOne);

            // SP++
            this.IncrementStackPointerCommand();

            return;
        }

        private void HandleGreaterThanCommand(int counter)
        {
            this.ComparisonBodyTemplate(counter, Constants.Mnemonics.Jumps.GreaterThan);
        }

        private void HandleLessThanCommand(int counter)
        {
            this.ComparisonBodyTemplate(counter, Constants.Mnemonics.Jumps.LessThan);
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
                            .Append(DReg.EqA);

            // RAM[SP] = index
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(MReg.EqD);

            // SP++
            this.IncrementStackPointerCommand();
        }

        private void HandlePushInStaticSegment(int index, string fileName)
        {
            string aInstructionForStaticVariable = $"{fileName}.{index}".ToAInstruction();

            this.HandlePushInTempOrStaticSegment(aInstructionForStaticVariable);
        }

        private void HandlePopInStaticSegment(int index, string fileName)
        {
            string aInstructionForStaticVariable = $"{fileName}.{index}".ToAInstruction();

            this.HandlePopInTempOrStaticSegment(aInstructionForStaticVariable);
        }

        private void HandlePushInTempSegment(int index)
        {
            string aInstructionForRegister = $"R{index + Constants.DefaultTempRegisterIndex}".ToAInstruction();

            this.HandlePushInTempOrStaticSegment(aInstructionForRegister);
        }

        private void HandlePopInTempSegment(int index)
        {
            string aInstructionForRegister = $"R{index + Constants.DefaultTempRegisterIndex}".ToAInstruction();

            this.HandlePopInTempOrStaticSegment(aInstructionForRegister);
        }

        private void HandlePushInTempOrStaticSegment(string aInstructionForSegment)
        {
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store RAM[Address] in D register
            this.transformed.Append(aInstructionForSegment)
                            .Append(DReg.EqM);

            // RAM[SP] = RAM[address]
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(MReg.EqD);

            this.IncrementStackPointerCommand();
        }

        private void HandlePopInTempOrStaticSegment(string aInstructionForSegment)
        {
            // SP--
            this.DecrementStackPointerCommand();

            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store RAM[SP] in the D register
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqM);

            // RAM[address] = RAM[SP]
            this.transformed.Append(aInstructionForSegment)
                            .Append(MReg.EqD);
        }

        private void HandlePushInPointerSegment(int index)
        {
            string aInstructionForPointer = index.PointerIndexToMnemonicMemorySegment().ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store RAM[THIS/THAT] in D register
            this.transformed.Append(aInstructionForPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqA);

            // RAM[SP] = RAM[THIS/THAT] (stored in D register)
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(MReg.EqD);

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
                            .Append(AReg.EqM)
                            .Append(DReg.EqM);

            // THIS/THAT = RAM[SP]
            this.transformed.Append(aInstructionForPointer)
                            .Append(MReg.EqD);
        }

        private void HandlePushInMemorySegment(int index, string segmentMnemonic)
        {
            string aInstructionForIndex = $"{index}".ToAInstruction();
            string aInstructionForSegment = segmentMnemonic.ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store (LCL|ARG|THIS|THAT + Index) in D register
            this.transformed.Append(aInstructionForIndex)
                            .Append(DReg.EqA)
                            .Append(aInstructionForSegment)
                            .Append(AReg.EqDPlusM)
                            .Append(DReg.EqM);

            // RAM[SP] = RAM[LCL|ARG|THIS|THAT + Index]
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(MReg.EqD);

            // SP++
            this.IncrementStackPointerCommand();
        }

        private void HandlePopInMemorySegment(int index, string segmentMnemonic)
        {
            // SP--
            this.DecrementStackPointerCommand();

            string aInstructionForIndex = $"{index}".ToAInstruction();
            string aInstructionForSegment = segmentMnemonic.ToAInstruction();
            string aInstructionForR13 = "R13".ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store (LCL|ARG|THIS|THAT + Index) in a free register R13
            this.transformed.Append(aInstructionForIndex)
                            .Append(DReg.EqA)
                            .Append(aInstructionForSegment)
                            .Append(DReg.EqDPlusM)
                            .Append(aInstructionForR13)
                            .Append(MReg.EqD);

            // Store RAM[SP] in the D register
            // Retrieve the pointer (LCL|ARG|THIS|THAT + Index) from R13
            // RAM[LCL|ARG|THIS|THAT + Index] = RAM[SP]
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqM)
                            .Append(aInstructionForR13)
                            .Append(AReg.EqM)
                            .Append(MReg.EqD);
        }
        #endregion

        #region Common commands
        private void IncrementStackPointerCommand()
        {
            this.transformed.Append(Constants.Mnemonics.StackPointer.ToAInstruction())
                            .Append(MReg.EqMPlusOne);
        }

        private void DecrementStackPointerCommand()
        {
            this.transformed.Append(Constants.Mnemonics.StackPointer.ToAInstruction())
                            .Append(MReg.EqMMinusOne);
        }

        // Handles GT & LT
        private void ComparisonBodyTemplate(int counter, string jumpMnemonic)
        {
            this.DecrementStackPointerCommand();

            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            // Store last value from the stack in the D register
            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqM);

            this.DecrementStackPointerCommand();

            this.transformed.Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(DReg.EqMMinusD);

            this.ComparisonLabelTemplate(counter, jumpMnemonic);

            this.IncrementStackPointerCommand();
        }

        private void ComparisonLabelTemplate(int counter, string jumpMnemonic)
        {
            string positiveLabel = $"POSITIVE.{counter}";
            string negativeLabel = $"NEGATIVE.{counter}";
            string endLabel = $"END.{counter}";

            string positiveLabelSymbolDeclaration = positiveLabel.ToLabelSymbolDeclaration();
            string negativeLabelSymbolDeclaration = negativeLabel.ToLabelSymbolDeclaration();
            string endLabelSymbolDeclaration = endLabel.ToLabelSymbolDeclaration();

            string aInstructionForPositiveScenario = positiveLabel.ToAInstruction();
            string aInstructionForNegativeScenario = negativeLabel.ToAInstruction();
            string aInstructionForEnd = endLabel.ToAInstruction();

            string aInstructionForR13 = "R13".ToAInstruction();
            string aInstructionForStackPointer = Constants.Mnemonics.StackPointer.ToAInstruction();

            string dPostComparisonJumpCommand = "D".ToJumpCommand(jumpMnemonic);
            string unconditionalJumpCommand = "0".ToJumpCommand(Constants.Mnemonics.Jumps.Uncoditional);

            this.transformed.Append(aInstructionForPositiveScenario)
                            .Append(dPostComparisonJumpCommand);

            this.transformed.Append(aInstructionForNegativeScenario)
                            .Append(unconditionalJumpCommand);

            // Positive outcome
            this.transformed.Append(positiveLabelSymbolDeclaration)
                            .Append(aInstructionForR13)
                            .Append(MReg.EqMinusOne) // true
                            .Append(DReg.EqM)
                            .Append(aInstructionForEnd)
                            .Append(unconditionalJumpCommand);

            // Negative outcome
            this.transformed.Append(negativeLabelSymbolDeclaration)
                            .Append(aInstructionForR13)
                            .Append(MReg.EqZero) // false
                            .Append(DReg.EqM)
                            .Append(aInstructionForEnd)
                            .Append(unconditionalJumpCommand);

            this.transformed.Append(endLabelSymbolDeclaration)
                            .Append(aInstructionForStackPointer)
                            .Append(AReg.EqM)
                            .Append(MReg.EqD);
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

