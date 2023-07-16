namespace HackAssembler.Exceptions
{
    public class UnexpectedCommandTypeException : Exception
    {
		public UnexpectedCommandTypeException()
		{
		}

        public UnexpectedCommandTypeException(string message)
        : base(message)
        {
        }

        public UnexpectedCommandTypeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

