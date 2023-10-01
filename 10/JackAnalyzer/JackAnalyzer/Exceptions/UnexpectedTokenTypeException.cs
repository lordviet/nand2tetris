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

        public UnexpectedTokenTypeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

