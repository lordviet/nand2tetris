using JackAnalyzer.Enums;

namespace JackAnalyzer.Contracts
{
    public interface IJackTokenizer
	{
		bool HasMoreTokens();

		void Advance();

		TokenType TokenType();

		Keyword Keyword();

		char Symbol();

		string Identifier();

		int IntegerValue();

		string StringValue();
	}
}

