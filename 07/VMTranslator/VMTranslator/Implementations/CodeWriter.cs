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

            if (segment == "local")
            {
                HandlePushPopInLocalSegment(commandType, index);

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

            string aInstructionForPointer = Constants.StackPointerMnemonic.ToAInstruction();

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

        private void HandlePushPopInLocalSegment(CommandType commandType, int index)
        {
            switch (commandType)
            {
                case CommandType.Push:
                    this.HandlePushInLocalSegment(index);
                    return;
                case CommandType.Pop:
                    this.HandlePopInLocalSegment(index);
                    return;
                default:
                    throw new NotSupportedException($"Unexpected command type '{commandType}'! Expected either '{CommandType.Push}' or '{CommandType.Pop}'.");
            };
        }

        private void HandlePushInConstantSegment(int index)
        {
            string aInstructionForIndex = $"{index}".ToAInstruction();
            string aInstructionForPointer = Constants.StackPointerMnemonic.ToAInstruction();

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

        private void HandlePushInLocalSegment(int index)
        {
            //string aInstructionForIndex = $"@{index}\n";
            //string aInstructionForPointer = $"@{Constants.StackPointerMnemonic}\n";

            //this.transformed.Append(aInstructionForIndex)
            //                .Append(DRegEqA)
            //                .Append(aInstructionForPointer)
            //                .Append(ARegEqM)
            //                .Append(MRegEqD);

            this.IncrementStackPointerCommand();
        }

        // TODO: think about this one?
        private void HandlePopInLocalSegment(int index)
        {
            // SP--
            this.DecrementStackPointerCommand();

            string aInstructionForIndex = $"{index}".ToAInstruction();
            string aInstructionForLocalSegment = Constants.LocalSegmentMnemonic.ToAInstruction();
            string aInstructinoForR13 = "R13".ToAInstruction();
            string aInstructionForStackPointer = Constants.StackPointerMnemonic.ToAInstruction();


            // Store (LCL + Index) in a free register R13
            this.transformed.Append(aInstructionForIndex)
                            .Append(DRegEqA)
                            .Append(aInstructionForLocalSegment)
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
            this.transformed.Append(Constants.StackPointerMnemonic.ToAInstruction())
                            .Append(MPlusOne);
        }

        private void DecrementStackPointerCommand()
        {
            this.transformed.Append(Constants.StackPointerMnemonic.ToAInstruction())
                            .Append(MMinusOne);
        }
        #endregion
    }
}

