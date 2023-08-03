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
                string aInstruction = $"@{index}\n";

                this.transformed.Append(aInstruction)
                                .Append(DRegEqA);

                return;
            }

            // TODO: Implement remaining segments and introduce a list of valid segments

            throw new NotImplementedException();
        }

        public string Close()
        {
            return this.transformed.ToString();
        }
    }
}

