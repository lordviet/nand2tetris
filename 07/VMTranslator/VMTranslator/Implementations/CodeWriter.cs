using System.Text;
using VMTranslator.Contracts;
using VMTranslator.Enums;
using VMTranslator.Extensions;

namespace VMTranslator.Implementations
{
    public class CodeWriter : ICodeWriter
	{
        private readonly StringBuilder transformed;

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

        public void WritePushPop(CommandType command, string segment, int index)
        {
            // TODO: Translate code
            throw new NotImplementedException();
        }

        public void SetFileName(string fileName)
        {
            throw new NotImplementedException();
        }

        public string Close()
        {
            return this.transformed.ToString();
        }
    }
}

