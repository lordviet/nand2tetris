using VMTranslator.Enums;

namespace VMTranslator.Contracts
{
    public interface ICodeWriter
	{
		void SetFileName(string fileName);

		void WriteCommentedOutInstruction(string instrction);

		void WriteArithmetic(string command);

		void WritePushPop(CommandType command, string segment, int index);

		string Close();

		// TODO: To be extended with more routines
	}
}

