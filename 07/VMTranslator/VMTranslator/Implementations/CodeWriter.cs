using System.Text;
using VMTranslator.Contracts;
using VMTranslator.Enums;
using VMTranslator.Extensions;

namespace VMTranslator.Implementations
{
    public class CodeWriter : ICodeWriter
    {
        private readonly StringBuilder transformed;

        // TODO: introduce common commands
        private const string DRegEqA = "D=A\n";
        private const string ARegEqM = "A=M\n";
        private const string MRegEqD = "M=D\n";

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
            throw new NotImplementedException();
        }

        public void WritePushPop(CommandType commandType, string segment, int index)
        {
            if (segment == "constant")
            {
                HandlePushPopInConstantSegment(commandType, index);

                return;
            }

            // TODO: Implement remaining segments and introduce a list of valid segments

            throw new NotImplementedException();
        }

        public string Close()
        {
            return this.transformed.ToString();
        }

        private void HandlePushPopInConstantSegment(CommandType commandType, int index)
        {
            switch (commandType)
            {
                case CommandType.Push:
                    HandlePushInConstantSegment(index);
                    return;
                case CommandType.Pop:
                    HandlePopInConstantSegment(index);
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

        private void IncrementStackPointerCommand()
        {
            string aInstruction = $"@{Constants.StackPointerMnemonic}\n";

            this.transformed.Append(aInstruction)
                            .Append(MPlusOne);
        }

        private void DecrementStackPointerCommand()
        {
            this.transformed.Append($"@{Constants.StackPointerMnemonic}")
                            .Append(MMinusOne);
        }
    }
}

