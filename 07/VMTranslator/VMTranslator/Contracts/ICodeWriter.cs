using VMTranslator.Enums;

namespace VMTranslator.Contracts
{
    public interface ICodeWriter
	{
		void WriteCommentedOutInstruction(string instrction);

		void WriteArithmetic(string command, int counter);

		void WritePushPop(CommandType commandType, string segment, int index);

		string Close();

		// TODO: To be extended with more routines
	}
}

