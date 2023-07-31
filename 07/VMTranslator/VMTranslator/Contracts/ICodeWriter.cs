using VMTranslator.Enums;

namespace VMTranslator.Contracts
{
    public interface ICodeWriter
	{
		void SetFileName(string fileName);

		void WriteArithmetic(string command);

		void WritePushPop(CommandType command, string segment, int index);

		void Close();

		// TODO: To be extended with more routines
	}
}

