using JackAnalyzer.Enums;

namespace JackAnalyzer.Exceptions
{
    public class UnexpectedTokenTypeException : Exception
    {
        public UnexpectedTokenTypeException()
        {
        }

        public UnexpectedTokenTypeException(string message)
            : base(message)
        {

        }

        public UnexpectedTokenTypeException(TokenType expected, TokenType received)
            : base($"Expected token of type - {expected}, instead got {received}.")
        {
        }

        public UnexpectedTokenTypeException(TokenType expected, TokenType received, Exception inner)
            : base($"Expected token of type - {expected}, instead got {received}. {inner}")
        {
        }
    }
}

