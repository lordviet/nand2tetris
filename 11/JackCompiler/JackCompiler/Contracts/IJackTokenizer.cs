using JackCompiler.Enums;

namespace JackCompiler.Contracts
{
    public interface IJackTokenizer
	{
		string GetCurrentToken();

		bool HasMoreTokens();

		void Advance();

		TokenType TokenType();

		Keyword Keyword();

		char Symbol();

		//string Identifier();

		int IntegerValue();

		string StringValue();
	}
}

