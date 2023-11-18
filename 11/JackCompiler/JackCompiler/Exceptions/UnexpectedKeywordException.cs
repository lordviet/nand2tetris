using JackCompiler.Enums;

namespace JackCompiler.Exceptions
{
    public class UnexpectedKeywordException : Exception
    {
        public UnexpectedKeywordException()
        {
        }

        public UnexpectedKeywordException(Keyword expected, Keyword received)
            : base($"Expected keyword - {expected}, instead got {received}.")
        {
        }

        public UnexpectedKeywordException(Keyword expected, Keyword received, Exception inner)
            : base($"Expected keyword - {expected}, instead got {received}. {inner}")
        {
        }
    }
}

