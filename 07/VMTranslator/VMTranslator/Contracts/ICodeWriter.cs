using VMTranslator.Enums;

namespace VMTranslator.Contracts
{
    public interface ICodeWriter
	{
		void WriteCommentedOutInstruction(string instrction);

		void WriteArithmetic(string command, int counter);

		void WritePushPop(CommandType commandType, string segment, int index);

		void WriteInit();

		void WriteLabel(string label);

		void WriteGoto(string label);

		void WriteIf(string label);

		string Close();

		// TODO: To be extended with more routines
	}
}

