using JackAnalyzer.Contracts;
using JackAnalyzer.Enums;

namespace JackAnalyzer.Implementations
{
    public class JackTokenizer : IJackTokenizer
	{
		public JackTokenizer()
		{
		}

        public bool HasMoreTokens()
        {
            throw new NotImplementedException();
        }

        public void Advance()
        {
            throw new NotImplementedException();
        }

        public TokenType TokenType()
        {
            throw new NotImplementedException();
        }

        public Keyword Keyword()
        {
            throw new NotImplementedException();
        }

        public char Symbol()
        {
            throw new NotImplementedException();
        }

        public string Identifier()
        {
            throw new NotImplementedException();
        }

        public int IntegerValue()
        {
            throw new NotImplementedException();
        }

        public string StringValue()
        {
            throw new NotImplementedException();
        }
    }
}

